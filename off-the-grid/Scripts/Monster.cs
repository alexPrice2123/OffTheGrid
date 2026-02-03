using Godot;
using System;

public partial class Monster : CharacterBody3D
{
	protected float _wanderSpeed;
	protected float _chaseSpeed;
	private NavigationAgent3D _navAgent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
	}

	private void _on_detection_body_entered(Node3D area)
	{
		if(area is Player player)
        {
            _navAgent.TargetPosition = Player.Instance.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
