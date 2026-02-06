using Godot;
using System;
using System.Net;

public partial class Monster : CharacterBody3D
{
	public bool _playerSeen = false;
	protected bool _playerHeard = false;
	protected bool _playerKnown = false;
	protected const float _chaseSpeed = 3; // chase speed is for when _playerSeen = true
	protected const float _regSpeed = 3.75f; // regular speed is for when the moster is wandering or when the player is cranking (_playerHeard = true)
	protected const float _knowSpeed = 4; // know speed is for when the player is on the red cam
	protected Vector3 _wanderPos;
	protected Vector3 _targetVelocity;
	protected Vector3 _dimensions;
	private NavigationAgent3D _navAgent;
	private CollisionShape3D _detArea;
	private RandomNumberGenerator _rng = new();
	private fusePathHandler _world;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_detArea = GetNode<CollisionShape3D>("IDetection/CollisionShape3D");
		_rng.Randomize();
		_world = (fusePathHandler)GetParent();
		_dimensions = _world.GetAllAabb((Node3D)GetParent()).Size;
		GetNewWanderPos();
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
		if (Player.Instance._currentCam.Equals(Player.Instance._otherCam)) { _playerKnown = true; } else if (Input.IsActionPressed("Crank")) { _playerHeard = true; }
		else { _playerKnown = false; _playerHeard = false; } // if the redcam is on or the player is crankng it set their respective variables to true

		_navAgent.TargetPosition = Player.Instance.GlobalPosition;
		Vector3 nextPoint = _navAgent.GetNextPathPosition();
		if (_playerHeard) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _regSpeed; }
		else if (_playerSeen) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _chaseSpeed; }
		else if (_playerKnown) { _targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized() * _knowSpeed; }
		else { _navAgent.TargetPosition = _wanderPos; } 

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);
		MoveAndSlide();
	}

	private void GetNewWanderPos()
	{
		float randZ = _rng.RandiRange(-15, 15);
		float randX = _rng.RandiRange(-15, 15);
		_wanderPos = new Vector3(randX, 0, randZ) * _regSpeed;
		GD.Print(_wanderPos);
	}
}
