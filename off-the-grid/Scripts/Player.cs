using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	private Node3D _head;
	private Camera3D _cam;
	public override void _Ready()
    {
       _head = GetNode<Node3D>("Head");
	   _cam = GetNode<Camera3D>("Head/Camera3D");
	   Input.MouseMode = Input.MouseModeEnum.Captured;
    }
	public override void _Input(InputEvent @event)
	{
		// --- Camera look ---
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			 // Rotate the character body on the Y-axis for horizontal look (yaw)
            RotateY(-motion.Relative.X * 0.002f);

            // Rotate the head/camera on the X-axis for vertical look (pitch)
            // Need to use a temp variable to modify the struct value
            Vector3 headRotation = _head.Rotation;
            headRotation.X += -motion.Relative.Y * 0.002f;
            
            // Clamp the vertical rotation
            headRotation.X = Mathf.Clamp(headRotation.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
            
            _head.Rotation = headRotation;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("Left", "Right", "Up", "Down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
