using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float WalkSpeed = 5.0f;
	private const float CrouchSpeed = 2.5f;
	private float _currentSpeed;
	private Node3D _head;
	private Camera3D _cam;
	private Node3D _walkPos;
	private Node3D _crouchPos;
	private Node3D _currentHeadPos;
	private CollisionShape3D _walkCollision;
	private CollisionShape3D _crouchCollision;
	private Camera3D _wallCam;
	private Camera3D _furnCam;
	private Camera3D _otherCam;
	private Camera3D _currentCam;
	public override void _Ready()
    {
       _head = GetNode<Node3D>("Head");
	   _cam = GetNode<Camera3D>("Head/Camera3D");
	   _walkPos = GetNode<Node3D>("WalkingHead");
	   _crouchPos = GetNode<Node3D>("CrouchHead");
	   _walkCollision = GetNode<CollisionShape3D>("WalkShape");
	   _crouchCollision = GetNode<CollisionShape3D>("CrouchShape");
	   _currentHeadPos = _walkPos;
	   _currentSpeed = WalkSpeed;
	   _wallCam = GetNode<Camera3D>("Head/Screen/SubViewport/WallCamera");
	   _furnCam = GetNode<Camera3D>("Head/Screen/SubViewport/FunitureCamera");
	   _otherCam = GetNode<Camera3D>("Head/Screen/SubViewport/OtherCamera");
	   _currentCam = _wallCam;
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

		_currentCam.GlobalPosition = GetNode<MeshInstance3D>("Head/Screen").GlobalPosition;
		_currentCam.GlobalRotation = _cam.GlobalRotation;
		if (Input.IsActionJustPressed("ToggleCamera"))
        {
			_currentCam.Current = false;
            if (_currentCam == _wallCam)
            {
                _currentCam = _furnCam;
            }
			else if (_currentCam == _furnCam)
            {
                _currentCam = _otherCam;
            }
            else
            {
                _currentCam = _wallCam;
            }
			_currentCam.Current = true;
        }

		if (Input.IsActionJustPressed("Crouch") && IsOnFloor())
        {
            if (_currentHeadPos == _walkPos)
			{
				_currentHeadPos = _crouchPos;
			 	_currentSpeed = CrouchSpeed;
				_walkCollision.Disabled = true;
				_crouchCollision.Disabled = false;
			}
			else
			{
				_currentHeadPos = _walkPos;
			 	_currentSpeed = WalkSpeed;
				_walkCollision.Disabled = false;
				_crouchCollision.Disabled = true;
			}
        }

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("Left", "Right", "Up", "Down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * _currentSpeed;
			velocity.Z = direction.Z * _currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _currentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _currentSpeed);
		}

		_head.Position = _head.Position.Lerp(_currentHeadPos.Position, (float)delta*5);

		Velocity = velocity;
		MoveAndSlide();
	}
}
