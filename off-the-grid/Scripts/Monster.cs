using Godot;
using System;

public partial class Monster : CharacterBody3D
{
	private Player _player;
	protected bool _playerSeen;
	protected Vector3 _targetVelocity;
	private NavigationAgent3D _navAgent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
	}

	private void _on_detection_body_entered(Node3D area)
	{
		if (area is Player player)
		{
			_player = player;
			if (!player.Velocity.Equals(0))
			{
				_playerSeen = true;
			}
			else
			{
				_playerSeen = false;
			}
		}
	}
	private void _on_seen_range_body_exited(Node3D area) { if (area is Player) { _playerSeen = false; GD.Print(_player.Name); } }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (_playerSeen)
		{
			_navAgent.TargetPosition = _player.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			_targetVelocity = (nextPoint - GlobalTransform.Origin).Normalized();
			GD.Print(_player.Name);
		}
		else
        {
			_targetVelocity = new Vector3(0, 0, 0);
        }

		_targetVelocity = new Vector3(_targetVelocity.X, -9.8f, _targetVelocity.Z);
		Velocity = Velocity.Lerp(_targetVelocity, 4f * (float)delta);
		MoveAndSlide();
	}
}
