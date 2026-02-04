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
	public Camera3D _otherCam;
	public Camera3D _currentCam;
	private MeshInstance3D _screen;
	private Node3D _defScreenPos;
	private Node3D _lookScreenPos;
	private Node3D _currentScreenPos;
	private float _power = 0f;
	private ShaderMaterial _screenMat;
	private Color _screenColor = new Color(0, 1, 0, 1);
	private RayCast3D _rayCast;
	public bool _hasFuse = false;
	private Node3D _currentObj;
	public int _collectedFuses = 0;
	public Control _pauseMenu;
	public Control _crosshair;
	public float _mouseSense = 0.002f;
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
		_rayCast = GetNode<RayCast3D>("Head/Camera3D/RayCast");
		_pauseMenu = GetNode<Control>("UI/Pause");
		_crosshair = GetNode<Control>("UI/Crosshair");
		Instance = this;
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }
	public override void _Input(InputEvent @event)
	{
		// --- Camera look ---
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			 // Rotate the character body on the Y-axis for horizontal look (yaw)
            RotateY(-motion.Relative.X * _mouseSense);

            // Rotate the head/camera on the X-axis for vertical look (pitch)
            // Need to use a temp variable to modify the struct value
            Vector3 headRotation = _head.Rotation;
            headRotation.X += -motion.Relative.Y * _mouseSense;
            
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
		_mouseSense = (float)_pauseMenu.GetNode<HSlider>("Mouse").Value/1000f;
		if (Input.IsActionPressed("Crank")) { _power += (float)delta * 0.2f; }
		else { _power -= (float)delta * 0.05f; }
		if (_power >= 2f) { _power = 2f; }
		if (_power <= 0f) { _power = 0f; }
		_screenMat.SetShaderParameter("intensity", _power);
		_screenMat.SetShaderParameter("hue", _screenColor);
		_screen.GetNode<Label3D>("Power").Modulate = _screenColor / 1.5f;
		if (Input.IsActionJustPressed("LookCamera")) { _currentScreenPos = _lookScreenPos; }
		if (Input.IsActionJustReleased("LookCamera")) { _currentScreenPos = _defScreenPos; }
		if (Input.IsActionJustPressed("WallCam"))
		{
			_currentCam.Current = false;
			_currentCam = _wallCam;
			_screenColor = new Color(0, 1, 0, 1);
			_currentCam.Current = true;
		}
		if (Input.IsActionJustPressed("FurnCam"))
		{
			_currentCam.Current = false;
			_currentCam = _furnCam;
			_screenColor = new Color(180f / 255f, 188f / 255f, 237f / 255f, 1);
			_currentCam.Current = true;
		}
		if (Input.IsActionJustPressed("OtherCam"))
		{
			_currentCam.Current = false;
			_currentCam = _otherCam;
			_screenColor = new Color(250f / 255f, 192f / 255f, 192f / 255f, 1);
			_currentCam.Current = true;
		}

		if (Input.IsActionJustPressed("CrouchToggle") && IsOnFloor()) { Crouch(_currentHeadPos == _walkPos); }

		if (Input.IsActionJustPressed("CrouchHold") && IsOnFloor()) { Crouch(true); }
		if (Input.IsActionJustReleased("CrouchHold") && IsOnFloor()) { Crouch(false); }

		CheckRaycast("Fuse");
		CheckRaycast("FuseBox");

		if (Input.IsActionJustPressed("Pause"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
			_pauseMenu.Visible = true;
			_crosshair.Visible = false;
			GetTree().Paused = true;
        }
		
		if (Input.IsActionJustPressed("Interact"))
		{
			if (_currentObj != null && !_hasFuse && _currentObj.IsInGroup("Fuse"))
			{
				Highlight(false, _currentObj.GetNode<MeshInstance3D>("Fuse"));
				_currentObj.QueueFree();
				_currentObj = null;
				_hasFuse = true;
			}
			if (_currentObj != null && _hasFuse && _currentObj.IsInGroup("FuseBox"))
            {
                Highlight(false, _currentObj.GetNode<MeshInstance3D>("FuseBox"));
				_currentObj = null;
				_hasFuse = false;
				_collectedFuses++;
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

		_head.Position = _head.Position.Lerp(_currentHeadPos.Position, (float)delta * 5);
		_screen.Position = _screen.Position.Lerp(_currentScreenPos.Position, (float)delta * 5);
		_screen.GetNode<Label3D>("Power").Text = "Power: " + Mathf.Floor(100 * _power / 2) + "%";

		Velocity = velocity;
		MoveAndSlide();
	}

	private void Crouch(bool toggle)
	{
		if (toggle)
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

	private Node3D CheckInteraction(string group)
	{
		if (_rayCast.IsColliding())
		{
			Node collider = (Node)_rayCast.GetCollider();

			if (collider != null)
			{
				if (collider.GetParent().IsInGroup(group))
				{
					return collider.GetParent() as Node3D;

				}
			}
		}
		return null;
	}

	private void Highlight(bool toggle, MeshInstance3D obj)
	{
		if (obj.MaterialOverlay is StandardMaterial3D material)
		{
			material.StencilColor = new Color(1, 1, 1, Convert.ToInt32(toggle));
		}
	}
	
	private void CheckRaycast(string objName)
	{
		if (objName == "Fuse" && _hasFuse) { return; }
		if (objName == "FuseBox" && !_hasFuse){ return; }
        if (CheckInteraction(objName) != null)
		{
			_currentObj = CheckInteraction(objName);
			Highlight(true, _currentObj.GetNode<MeshInstance3D>(objName));
		}
		else if (_currentObj != null)
		{
			Highlight(false, _currentObj.GetNode<MeshInstance3D>(objName));
			_currentObj = null;
		}
    }
}
