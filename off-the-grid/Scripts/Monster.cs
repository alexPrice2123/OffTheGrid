using Godot;
using System;
using System.Net;

public partial class Monster : CharacterBody3D
{
	protected bool _playerSeen = false;
	protected bool _playerHeard = false;
	protected bool _playerKnown = false;
	protected const float _chaseSpeed = 3;
	protected const float _regSpeed = 3.75f;
	protected const float _knowSpeed = 4;
	protected Vector3 _wanderPos;
	protected Vector3 _targetVelocity;
	protected RandomNumberGenerator _rng;
	private Vector3 _dimensions; 
	private NavigationAgent3D _navAgent;
	private CollisionShape3D _detArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_detArea = GetNode<CollisionShape3D>("IDetection/CollisionShape3D");
		_rng.Randomize();
		_dimensions = fusePathHandler.Instance.GetAllAabb((Node3D)ResourceLoader.Load<PackedScene>("res://Scenes/world.tscn").Instantiate()).Size;
		GetNewWanderPos();
	}

	private void _on_detection_body_entered(Node3D area)
	{
		if (area is Player)
		{
			_playerSeen = true;
			if (_detArea.Shape is SphereShape3D sphere)
			{
				sphere.Radius = 12;
			}
		}
	}
	private void _on_detection_body_exited(Node3D area) { if (area is Player) { _playerSeen = false; if (_detArea.Shape is SphereShape3D sphere) { sphere.Radius = 4.5f; } } }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Player.Instance._currentCam.Equals(Player.Instance._otherCam)) { _playerKnown = true; } else if (Input.IsActionPressed("Crank")) { _playerHeard = true; } 
		else { _playerKnown = false; _playerHeard = false; }

		_navAgent.TargetPosition = Player.Instance.GlobalPosition;
		Vector3 nextPoint = _navAgent.GetNextPathPosition();
		if (_playerHeard) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _regSpeed; }
		else if (_playerSeen) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _chaseSpeed; }
		else if (_playerKnown) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _knowSpeed; }
		else { _targetVelocity = _wanderPos; }

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);
		MoveAndSlide();
	}

	private void GetNewWanderPos()
	{
		float randZ = _rng.RandiRange(-(int)_dimensions.Z, (int)_dimensions.Z);
		float randX = _rng.RandiRange(-(int)_dimensions.X, (int)_dimensions.X);
		_wanderPos = new Vector3(randX, 0, randZ) * _regSpeed;
	}
}
