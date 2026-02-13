using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public PackedScene GlowstickScene { get; set; }
	public static Player Instance { get; private set; }
	private const float WalkSpeed = 5.0f;
	private const float CrouchSpeed = 2.5f;
	private const float BOB_FREQ = 4.0f;
	private const float BOB_AMP = 0.02f;
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
	public Node3D _defScreenPos;
	private Node3D _lookScreenPos;
	private Node3D _startScreenPos;
	public Node3D _currentScreenPos;
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
	private MeshInstance3D _mode1;
	private MeshInstance3D _mode2;
	private MeshInstance3D _mode3;
	private MeshInstance3D _fuseLight;
	private MeshInstance3D _power1;
	private MeshInstance3D _power2;
	private MeshInstance3D _power3;
	private MeshInstance3D _power4;
	private MeshInstance3D _light;
	private MeshInstance3D _crank;
	public bool _inTutorial = true;
	private float _bob = 0.0f;
	private Vector3 _initialCameraPosition;
	private Monster _monster;
	private AudioStreamPlayer _heartBeat;
	private int _count = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	public bool _hidden = false;
	private ShaderMaterial _heartMat;
	private StaticBody3D _currentHide;
	private float _disturbGoal = 0;
	private float _glowCD = 0;
	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
		_initialCameraPosition = _cam.Position;
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
		_startScreenPos = GetNode<Node3D>("Head/StartScreen");
		_currentCam = _wallCam;
		_currentScreenPos = _startScreenPos;
		_screenMat = _screen.MaterialOverride as ShaderMaterial;
		_rayCast = GetNode<RayCast3D>("Head/Camera3D/RayCast");
		_pauseMenu = GetNode<Control>("UI/Pause");
		_crosshair = GetNode<Control>("UI/Crosshair");
		_mode1 = GetNode<MeshInstance3D>("Head/Screen/Radar/M1");
		_mode2 = GetNode<MeshInstance3D>("Head/Screen/Radar/M2");
		_mode3 = GetNode<MeshInstance3D>("Head/Screen/Radar/M3");
		_fuseLight = GetNode<MeshInstance3D>("Head/Screen/Radar/Fuse");
		_power1 = GetNode<MeshInstance3D>("Head/Screen/Radar/PL1");
		_power2 = GetNode<MeshInstance3D>("Head/Screen/Radar/PL2");
		_power3 = GetNode<MeshInstance3D>("Head/Screen/Radar/PL3");
		_power4 = GetNode<MeshInstance3D>("Head/Screen/Radar/PL4");
		_light = GetNode<MeshInstance3D>("Head/Screen/Radar/Light");
		_crank = GetNode<MeshInstance3D>("Head/Screen/Radar/crank");
		_monster = GetParent().GetNode<Monster>("Monster");
		_heartBeat = GetNode<AudioStreamPlayer>("Heartbeat");
		_heartMat = GetNode<ColorRect>("UI/Heart").Material as ShaderMaterial;
		Instance = this;
		LightHandler(true, _mode1);
		_screenColor = new Color(0, 1.25f, 0, 1);
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _Input(InputEvent @event)
	{
		// --- Camera look ---
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured && !_inTutorial)
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
		float _dist = (_monster.GlobalPosition - GlobalPosition).Length();
		if (_dist < 1.5 || GetNode<Control>("UI/GameOver").Visible)
		{
			_inTutorial = true;
			GetNode<Ui>("UI")._transitionGoal = 0.5f;
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music")) - 0.1f);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX"))-0.1f);
			if (!GetNode<Control>("UI/GameOver").Visible)
			{
				GetNode<AudioStreamPlayer>("UI/End").Play();
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			GetNode<Control>("UI/GameOver").Visible = true;
		}
		if (_dist < 5) { _heartBeat.PitchScale = 1.75f; }
		else if (_dist < 10) { _heartBeat.PitchScale = 1.5f; }
		else if (_dist < 15) { _heartBeat.PitchScale = 1.25f; }
		else { _heartBeat.PitchScale = 1; }
		if (_dist < 25 && !_heartBeat.Playing) { Beat(); }
		
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		_currentCam.GlobalPosition = GetNode<MeshInstance3D>("Head/Screen").GlobalPosition;
		_currentCam.GlobalRotation = _cam.GlobalRotation;
		_mouseSense = (float)_pauseMenu.GetNode<HSlider>("Mouse").Value / 1000f;
		if (_power >= 2f) { _power = 2f; }
		if (_power <= 0f) { _power = 0f; }
		if (_power >= 2)
		{
			LightHandler(true, _power4);
		}
		else if (_power >= 1.5f)
		{
			LightHandler(false, _power4);
			LightHandler(true, _power3);
		}
		else if (_power >= 1)
		{
			LightHandler(false, _power4);
			LightHandler(false, _power3);
			LightHandler(true, _power2);
		}
		else if (_power >= 0.01f)
		{
			LightHandler(false, _power4);
			LightHandler(false, _power3);
			LightHandler(false, _power2);
			LightHandler(true, _power1);
		}
		else
		{
			LightHandler(false, _power4);
			LightHandler(false, _power3);
			LightHandler(false, _power2);
			LightHandler(false, _power1);
		}
		LightHandler(_hasFuse, _fuseLight);
		LightHandler(GetNode<Node3D>("Head/Camera3D/Glowstick").Visible, _light);
		_screenMat.SetShaderParameter("intensity", _power);
		_screenMat.SetShaderParameter("hue", _screenColor);
		CheckRaycast("Fuse");
		CheckRaycast("FuseBox");
		if (!GetNode<Node3D>("Head/Camera3D/Glowstick").Visible)
        {
            _glowCD += (float)delta;
			if (_glowCD > 45f)
            {
                _glowCD = 0f;
				GetNode<Node3D>("Head/Camera3D/Glowstick").Visible = true;

            }
        }
		if (!_inTutorial)
		{
			float increaseAmount;
			if (velocity.Length() > 0.1f) { increaseAmount = 0.2f; }
			else{ increaseAmount = 0.3f; }
			if (Input.IsActionPressed("Crank")) { _power += (float)delta * increaseAmount; _crank.RotateZ(-0.1f); }
			_power -= (float)delta * 0.05f*(_disturbGoal+1.5f);
			if (Input.IsActionJustPressed("Crank")) { GetNode<AudioStreamPlayer>("Crank").Play(); }
			if (Input.IsActionJustReleased("Crank")) { GetNode<AudioStreamPlayer>("Crank").Stop(); }
			if (Input.IsActionJustPressed("LookCamera")) { _currentScreenPos = _lookScreenPos; }
			if (Input.IsActionJustReleased("LookCamera")) { _currentScreenPos = _defScreenPos; }
			if (Input.IsActionJustPressed("WallCam"))
			{
				LightHandler(true, _mode1);
				LightHandler(false, _mode2);
				LightHandler(false, _mode3);
				_currentCam.Current = false;
				_currentCam = _wallCam;
				_screenColor = new Color(0, 1.25f, 0, 1);
				_currentCam.Current = true;
			}
			if (Input.IsActionJustPressed("FurnCam"))
			{
				LightHandler(false, _mode1);
				LightHandler(true, _mode2);
				LightHandler(false, _mode3);
				_currentCam.Current = false;
				_currentCam = _furnCam;
				_screenColor = new Color(120f / 255f, 120f / 255f, 237f / 255f, 1);
				_currentCam.Current = true;
			}
			if (Input.IsActionJustPressed("OtherCam"))
			{	
				LightHandler(false, _mode1);
				LightHandler(false, _mode2);
				LightHandler(true, _mode3);
				_currentCam.Current = false;
				_currentCam = _otherCam;
				_screenColor = new Color(255f / 255f, 150f / 255f, 150f / 255f, 1);
				_currentCam.Current = true;
			}

			if (Input.IsActionJustPressed("CrouchToggle") && IsOnFloor()) { Crouch(_currentHeadPos == _walkPos); }
			if (Input.IsActionJustPressed("Throw") && IsOnFloor()) 
			{
				if (!GetNode<Node3D>("Head/Camera3D/Glowstick").Visible){return;}
				Glowstick instance = (Glowstick)GlowstickScene.Instantiate();
				instance._direction = -_cam.GlobalTransform.Basis.Z.Normalized();
				GetParent().AddChild(instance);
				instance.GlobalPosition = GetNode<Node3D>("Head/Camera3D/Throw").GlobalPosition;
				GetNode<Node3D>("Head/Camera3D/Glowstick").Visible = false;
			}

			if (Input.IsActionJustPressed("CrouchHold") && IsOnFloor()) { Crouch(true); }
			if (Input.IsActionJustReleased("CrouchHold") && IsOnFloor()) { Crouch(false); }
			
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
					_hasFuse = false;
					_collectedFuses++;
					GD.Print("Fuse" + _collectedFuses);
					_currentObj.GetNode<MeshInstance3D>("Fuse" + _collectedFuses).Visible = true;
					GetNode<AudioStreamPlayer>("Fuse").Play();
					if (_collectedFuses >= 5) { _currentObj.GetNode<GpuParticles3D>("Sparks").Emitting = false; }
					_currentObj = null;
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

			if (velocity.Length() > 0.1f && IsOnFloor())
			{
				_bob += (float)delta * velocity.Length() * 0.5f;
			}
			else
			{
				_bob = 0;
			}
			Vector3 bobOffset = CalculateHeadBob(_bob);
			_cam.Position = _cam.Position.Lerp(_initialCameraPosition + bobOffset, (float)delta*3);
		}

		_head.Position = _head.Position.Lerp(_currentHeadPos.Position, (float)delta * 5);
		_screen.Position = _screen.Position.Lerp(_currentScreenPos.Position, (float)delta * 5);

		Velocity = velocity;
		if (velocity.Length() > 1)
		{
			_count++;
			if (_count > 150 / _currentSpeed)
			{
				GetNode<AudioStreamPlayer>("Footstep").PitchScale = _rng.RandfRange(0.9f, 1.1f);
				GetNode<AudioStreamPlayer>("Footstep").Play();
				_count = 0;
			}
		}
		else { _count = 0; }
		_hidden = GetNode<Area3D>("Hide").GetOverlappingBodies().Count > 1;

		bool _distortCheck = false;
		foreach (Area3D area in GetNode<Area3D>("Hide").GetOverlappingAreas())
        {
            if(area.IsInGroup("Distort")){_distortCheck = true;}
        }
		if (!_hidden){_distortCheck = false;}
		if (_distortCheck){_disturbGoal = 1f; if (!GetNode<AudioStreamPlayer>("Whisper").Playing){GetNode<AudioStreamPlayer>("Whisper").Play();}}
		else{_disturbGoal = 0f; GetNode<AudioStreamPlayer>("Whisper").Stop();}
		if ((float)_screenMat.GetShaderParameter("disturbed") > 0)
        {
            AudioServer.SetBusEffectEnabled(AudioServer.GetBusIndex("Master"),0, true);
        }
        else
        {
            AudioServer.SetBusEffectEnabled(AudioServer.GetBusIndex("Master"),0, false);
        }
		if (AudioServer.GetBusEffect(AudioServer.GetBusIndex("Master"),0) is AudioEffectPhaser affectM)
        {
            affectM.Depth = (float)_screenMat.GetShaderParameter("disturbed")*4;
        }
		if (AudioServer.GetBusEffect(AudioServer.GetBusIndex("Master"),0) is AudioEffectPhaser affectS)
        {
            affectS.Depth = (float)_screenMat.GetShaderParameter("disturbed")*4;
        }

		_screenMat.SetShaderParameter("disturbed", Mathf.Lerp((float)_screenMat.GetShaderParameter("disturbed"), _disturbGoal, (float)delta/((26f*_disturbGoal)+4f)));

		MoveAndSlide();
	}

	private void Crouch(bool toggle)
	{
		if (_hidden){ return; }
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
		if (objName == "FuseBox" && !_hasFuse) { return; }
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

	private void LightHandler(bool toggle, MeshInstance3D lightRef)
	{
		if (lightRef.MaterialOverride is StandardMaterial3D material)
		{
			material.EmissionEnergyMultiplier = Convert.ToInt32(toggle) * 5;
		}
	}

	private Vector3 CalculateHeadBob(float time)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = Mathf.Sin(time * BOB_FREQ) * BOB_AMP;
		pos.X = Mathf.Cos(time * BOB_FREQ / 2.0f) * BOB_AMP;
		return pos;
	}

	private async void Beat()
	{
		_heartBeat.Play();
		for (float i = 0; i <= 20; i++)
		{
			_heartMat.SetShaderParameter("beat", i / 20f);
			GD.Print(_heartMat.GetShaderParameter("beat"));
			await ToSignal(GetTree().CreateTimer(0.01f), SceneTreeTimer.SignalName.Timeout);
		}
	}
}
