using Godot;
using System;

public partial class Glowstick : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public Vector3 _direction;
	private bool _still = false;
	public async override void _Ready()
    {
        //LookAt(GlobalPosition + _direction, Vector3.Up);
        ApplyCentralImpulse(_direction * 20);
		await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
		_still = true;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (LinearVelocity.Length() <= 1 && _still)
        {
            GetNode<CollisionShape3D>("Small").Disabled = false;
			GetNode<CollisionShape3D>("Big").Disabled = true;
        }
		if (LinearVelocity.Length() <= 0.01 && _still)
        {
            GravityScale = 0f;
        }
		GetNode<OmniLight3D>("Light").LightEnergy -= (float)delta*5;
		if (GetNode<MeshInstance3D>("Stick").MaterialOverlay is StandardMaterial3D mat)
        {
            // disntacne fade stuff mat.DistanceFadeMaxDistance 
        }
		GD.Print((GetParent().GetNode<Player>("Player").GlobalPosition - GlobalPosition).Length());
    }
}
