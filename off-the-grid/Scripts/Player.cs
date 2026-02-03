using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public static Player Instance { get; private set; }
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
	private MeshInstance3D _screen;
	private Node3D _defScreenPos;
	private Node3D _lookScreenPos;
	private Node3D _currentScreenPos;
	private float _power = 0f;
	private ShaderMaterial _screenMat;
	private Color _screenColor = new Color(0, 1, 0, 1);
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
		_screen = GetNode<MeshInstance3D>("Head/Screen");
		_defScreenPos = GetNode<Node3D>("Head/DefScreen");
		_lookScreenPos = GetNode<Node3D>("Head/LookScreen");
		_currentCam = _wallCam;
		_currentScreenPos = _defScreenPos;
		_screenMat = _screen.MaterialOverride as ShaderMaterial;
		Instance = this;
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
		if (Input.IsActionPressed("Crank")) { _power += (float)delta * 0.2f; }
		else { _power -= (float)delta * 0.1f; }
		if (_power >= 2f) { _power = 2f; }
		if (_power <= 0f) { _power = 0f; }
		_screenMat.SetShaderParameter("intensity", _power);
		_screenMat.SetShaderParameter("hue", _screenColor);
		_screen.GetNode<Label3D>("Power").Modulate = _screenColor / 1.5f;
		if (Input.IsActionJustPressed("LookCamera")) { _currentScreenPos = _lookScreenPos; }
		if (Input.IsActionJustReleased("LookCamera")) { _currentScreenPos = _defScreenPos; }
		if (Input.IsActionJustPressed("ToggleCamera"))
        {
			_currentCam.Current = false;
            if (_currentCam == _wallCam)
            {
				_currentCam = _furnCam;
				_screenColor = new Color(180f/255f, 188f/255f, 237f/255f, 1);
            }
			else if (_currentCam == _furnCam)
            {
				_currentCam = _otherCam;
				_screenColor = new Color(250f/255f, 192f/255f, 192f/255f, 1);
            }
            else
            {
				_currentCam = _wallCam;
				_screenColor = new Color(0, 1, 0, 1);
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
		_screen.Position = _screen.Position.Lerp(_currentScreenPos.Position, (float)delta * 5);
		_screen.GetNode<Label3D>("Power").Text = "Power: " + Mathf.Floor(100*_power/2) + "%";
		
		Velocity = velocity;
		MoveAndSlide();
	}
}
