using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class fusePathHandler : Node3D
{
	[Export]
	public PackedScene PathMeshScene { get; set; }

	[Export]
	public PackedScene FuseScene { get; set; }

	[Export]
	public PackedScene DarkScene { get; set; }

	[Export]
	public PackedScene LightScene { get; set; }

	[Export]
	public float MeshSpacing { get; set; } = 1.0f;

	[Export]
	public int MaxFuses { get; set; } = 5;

	public static fusePathHandler Instance { get; private set; }
	private int _count = 0;
	private float _closestDist = 10000;
	private Node3D _closestFuse;
	private Rid _navigationMap;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private List<Node3D> _spawnedMeshes = new List<Node3D>();
	private List<Node3D> _spawnedFuses = new List<Node3D>();
	private Player _player;
	private int _maxFuseOG;
	public bool _isLight = true;
	public bool _lightsOff = false;
	private AudioStreamPlayer _sound;
	public override void _Ready()
	{
		Instance = this;
		_player = GetNode<Player>("Player");
		_sound = GetNode<AudioStreamPlayer>("Music");
		_maxFuseOG = MaxFuses;
		_rng.Randomize();
		_spawnedFuses = GetNode<Node3D>("FuseSpawns").GetChildren().OfType<Node3D>().ToList();
		_navigationMap = GetWorld3D().NavigationMap;
		while (MaxFuses > 0 && _spawnedFuses.Count > 0)
		{
			int chosen = _rng.RandiRange(0, _spawnedFuses.Count - 1);
			Node3D spawn = _spawnedFuses[chosen];

			Node3D meshInstance = (Node3D)FuseScene.Instantiate();
			GetNode<Node3D>("FuseHolder").AddChild(meshInstance);
			meshInstance.GlobalPosition = spawn.GlobalPosition;

			Lighting(LightScene, "Light");
			_isLight = true;

			_spawnedFuses.RemoveAt(chosen);
			MaxFuses--;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_player._inTutorial)
        {
           	AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), (float)GetNode<HSlider>("Player/UI/Pause/Music").Value);
			AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), (float)GetNode<HSlider>("Player/UI/Pause/Music").Value < -9);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), (float)GetNode<HSlider>("Player/UI/Pause/SFX").Value);
			AudioServer.SetBusMute(AudioServer.GetBusIndex("SFX"), (float)GetNode<HSlider>("Player/UI/Pause/SFX").Value < -9); 
        }
		_count += 2;
		if (_player._collectedFuses >= _maxFuseOG+1 && !_isLight)
		{
			Lighting(LightScene, "Light");
			_sound.Stop();
			_isLight = true;
			_player.GetNode<Ui>("UI")._currentLine = 9;
			_player.GetNode<Ui>("UI").Type();
			_player.GetNode<Ui>("UI")._text.Text = _player.GetNode<Ui>("UI")._lineTable[_player.GetNode<Ui>("UI")._currentLine];
			_player.GetNode<Control>("UI/Dialouge").Visible = true;
		}
		else if (_player._collectedFuses < _maxFuseOG+1 && _isLight && _lightsOff)
		{
			Lighting(DarkScene, "Dark");
			_sound.Play();
			_isLight = false;
		}

		if (_count == 100)
		{
			_closestDist = 10000;
			if (!_player._hasFuse)
			{
				foreach (Node3D fuse in GetNode<Node3D>("FuseHolder").GetChildren())
				{
					if ((_player.GlobalPosition - fuse.GlobalPosition).Length() < _closestDist)
					{
						_closestDist = (_player.GlobalPosition - fuse.GlobalPosition).Length();
						_closestFuse = fuse;
					}
				}
			}
			else
			{
				_closestFuse = GetNode<Node3D>("FuseBox");
			}
		   	GenerateAndPlaceMeshes(_player.GlobalPosition, _closestFuse.GlobalPosition); 
		}
		if (_count == 150)
		{
			foreach (GpuParticles3D warn in _spawnedMeshes)
			{
				warn.Emitting = false;
			}
			_count = 0;
		}
		if (_count == 200)
		{
			ClearMeshes();
			_count = 0;
		}
	}

	public void GenerateAndPlaceMeshes(Vector3 startPosition, Vector3 targetPosition)
	{
		Vector3[] pathPoints = NavigationServer3D.MapGetPath(
			_navigationMap,
			startPosition,
			targetPosition,
			true 
		);

		if (pathPoints.Length == 0)
		{
			GD.Print("Fail");
			return;
		}
		PlaceMeshesAlongPath(pathPoints);
	}

	private void PlaceMeshesAlongPath(Vector3[] pathPoints)
	{
		float distanceTraveled = 0f;
		
		for (int i = 0; i < pathPoints.Length - 1; i++)
		{
			Vector3 currentPos = pathPoints[i];
			Vector3 nextPos = pathPoints[i + 1];
			float segmentLength = currentPos.DistanceTo(nextPos);

			while (distanceTraveled < segmentLength)
			{
				Vector3 spawnPosition = currentPos.Lerp(nextPos, distanceTraveled / segmentLength);

				Node3D meshInstance = (Node3D)PathMeshScene.Instantiate();
				AddChild(meshInstance);
				meshInstance.GlobalPosition = spawnPosition;
				_spawnedMeshes.Add(meshInstance);
				
				distanceTraveled += MeshSpacing;
			}

			distanceTraveled -= segmentLength;
		}
	}

	private void ClearMeshes()
	{
		foreach (var mesh in _spawnedMeshes)
		{
			mesh.QueueFree();
		}
		_spawnedMeshes.Clear();
	}

	private void Lighting(PackedScene lightingScene, string sceneName)
	{
		Node3D holder = GetNode<Node3D>("EnviormentHolder");
		foreach (Node3D env in GetNode<Node3D>("EnviormentHolder").GetChildren()){ env.QueueFree(); }

		Node3D instance = (Node3D)lightingScene.Instantiate();
		instance.Name = sceneName;
		holder.AddChild(instance);
	}

	public Aabb GetAllAabb(Node3D parent)
	{
		Aabb _combinedAabb = new Aabb();
		bool first = true;

		foreach (Node child in GetChildren())
		{
			if (child is VisualInstance3D visualChild)
			{
				Aabb _childAabb = visualChild.GetAabb();
				Aabb transformedAabb = parent.GlobalTransform.Inverse() * visualChild.GlobalTransform * _childAabb;
				if (first)
				{
					_combinedAabb = transformedAabb;
					first = false;
				}
				else
				{
					_combinedAabb = _combinedAabb.Merge(transformedAabb);
				}
			}
		}
		return _combinedAabb;
	}
}
