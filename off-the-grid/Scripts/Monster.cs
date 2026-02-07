using Godot;
using System;
using System.Net;

public partial class Monster : CharacterBody3D
{
	protected bool _playerSeen = false;
	protected bool _playerHeard = false;
	private bool _playerKnown = false;
	protected const float _chaseSpeed = 3; // chase speed is for when _playerSeen = true
	protected const float _regSpeed = 3.75f; // regular speed is for when the moster is wandering or when the player is cranking (_playerHeard = true)
	protected const float _knowSpeed = 4; // know speed is for when the player is on the red cam
	protected Vector3 _wanderPos = new Vector3(0,0,0);
	protected Vector3 _targetVelocity;
	protected Vector3 _dimensions;
	private NavigationAgent3D _navAgent;
	private CollisionShape3D _detArea;
	private RandomNumberGenerator _rng = new();
	private fusePathHandler _world;
	public Vector3 _goalPos = new Vector3(0,0,676767);
	private int _count = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_detArea = GetNode<CollisionShape3D>("IDetection/CollisionShape3D");
		_rng.Randomize();
		_world = (fusePathHandler)GetParent();
		_dimensions = _world.GetAllAabb((Node3D)GetParent()).Size;
	}

	private void _on_detection_body_entered(Node3D area) // when the player is close enough
	{
		if (area is Player)
		{
			_playerSeen = true; // set _playerSeen to true until the player leaves the new extended radius
			if (_detArea.Shape is SphereShape3D sphere)
			{
				sphere.Radius = 16; // extend the radius so that the monster remains in persuit for longer
			}
		}
	}
	private void _on_detection_body_exited(Node3D area) { if (area is Player) { _playerSeen = false; if (_detArea.Shape is SphereShape3D sphere) { sphere.Radius = 4.5f; } } }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (_world.GetNode<Player>("Player")._inTutorial || _world._isLight){ return; }
		_count += 1;
		if ((_goalPos - GlobalPosition).Length() <= 1)
        {
            _goalPos = new Vector3(0,0,676767);
        }
		if (_count == 100) { GetNewWanderPos(); }
		if (_count == 250){ _count = 0; }
		if (Player.Instance._currentCam.Equals(Player.Instance._otherCam)) { _playerKnown = true; } else if (Input.IsActionPressed("Crank")) { _playerHeard = true; }
		else { _playerKnown = false; _playerHeard = false; } // if the redcam is on or the player is crankng it set their respective variables to true

		_navAgent.TargetPosition = Player.Instance.GlobalPosition;
		if (!_playerHeard && !_playerKnown && !_playerSeen)
		{
			_navAgent.TargetPosition = _wanderPos;
		}
		if (_goalPos != new Vector3(0, 0, 676767))
        {
			_navAgent.TargetPosition = _goalPos;
        }
		Vector3 nextPoint = _navAgent.GetNextPathPosition();
		if (_playerHeard) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _regSpeed; }
		else if (_playerSeen) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _chaseSpeed; }
		else if (_playerKnown || _goalPos != new Vector3(0, 0, 676767)) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _knowSpeed; }
		else { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _regSpeed; }

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);
		Vector3 hVel = Velocity;
		hVel.Y = 0;
		if (hVel.LengthSquared() > 3)
		{
			Vector3 targetPosition = GlobalPosition + hVel;
			LookAt(targetPosition, Vector3.Up);
		}
		MoveAndSlide();
	}

	private void GetNewWanderPos()
	{
		float angle = (float)GD.RandRange(0, Mathf.Tau);
        float radius = (float)GD.RandRange(0, 15);
		Vector3 randomPoint = GlobalPosition + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
		Rid mapRid = GetParent().GetNode<NavigationRegion3D>("NavigationRegion3D").GetNavigationMap();
		_wanderPos = NavigationServer3D.MapGetClosestPoint(mapRid, randomPoint);
	}
}
