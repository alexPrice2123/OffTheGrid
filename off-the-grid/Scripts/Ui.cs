using Godot;
using System;

public partial class Ui : Control
{
	[Export]
	public string[] _lineTable = {};
	public Label _text;
	private float _typingSpeed = 0.025f;
	public int _currentLine = 0;
	private int _currentCount = 0;
	private bool _inCutscene = false;
	private float _skipCount = 0;
	private int _tutPage = 1;
	private ShaderMaterial _transitionMat;
	public float _transitionGoal = -0.1f;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
		_text = GetNode<Label>("Dialouge/Text");
		_text.Text = _lineTable[_currentLine];
		_text.VisibleCharacters = 0;
		_transitionMat = GetNode<ColorRect>("Transition").Material as ShaderMaterial;
		await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
		Type();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_transitionMat.SetShaderParameter("fade", Mathf.Lerp((float)_transitionMat.GetShaderParameter("fade"), _transitionGoal, delta * 2));
		if ((float)_transitionMat.GetShaderParameter("fade") >= 1)
		{
			GetTree().ChangeSceneToFile("res://Scenes/credits.tscn");
		}
		if ((float)_transitionMat.GetShaderParameter("fade") <= 0)
		{
			_transitionGoal = 0;
		}
		if (_lineTable[_currentLine] == "Cutscene1" && !_inCutscene)
		{
			_inCutscene = true;
			GetNode<Control>("Dialouge").Visible = false;
			Cutscene1();
		}
		else if (_lineTable[_currentLine] == "Cutscene2" && !_inCutscene)
		{
			_inCutscene = true;
			GetNode<Control>("Dialouge").Visible = false;
			Cutscene2();
		}
		if (Input.IsActionJustPressed("Skip") && !_inCutscene)
		{
			if (GetNode<Control>("Dialouge").Visible == true)
			{
				if (GetParent().GetParent<fusePathHandler>()._isLight && !GetParent<Player>()._inTutorial)
				{
					if (11 <= _currentLine && (_currentCount >= _lineTable[_currentLine].Length))
					{
						_transitionGoal = 1.1f;
					}
					else
					{
						if (!(_currentCount >= _lineTable[_currentLine].Length))
						{
							_currentCount = _lineTable[_currentLine].Length;
						}
						else
						{
							_currentLine++;
							Type();
							_text.Text = _lineTable[_currentLine];
						} 
					} 
				}
				else
				{
					if (8 <= _currentLine && (_currentCount >= _lineTable[_currentLine].Length))
					{
						GetNode<Control>("Dialouge").Visible = false;
						GetParent<Player>()._currentScreenPos = GetParent<Player>()._defScreenPos;
						_skipCount = 0;
						GetNode<Control>("Tutorial").Visible = true;
					}
					else
					{
						if (!(_currentCount >= _lineTable[_currentLine].Length))
						{
							_currentCount = _lineTable[_currentLine].Length;
						}
						else
						{
							_currentLine++;
							Type();
							_text.Text = _lineTable[_currentLine];
						} 
					} 
				}
			}
			else if (GetNode<Control>("Tutorial").Visible == true)
			{
				if (_tutPage > 4)
				{
					GetNode<Control>("Tutorial").Visible = false;
					GetParent<Player>()._inTutorial = false;
				}
				else
				{
				   GetNode<Label>("Tutorial/Text"+_tutPage).Visible = false;
					_tutPage++; 
				}
			}
		}
		GetNode<Label>("Dialouge/Warn").Visible = _currentCount >= _lineTable[_currentLine].Length;
		if (Input.IsActionPressed("Skip") && !_inCutscene && !(GetParent().GetParent<fusePathHandler>()._isLight && !GetParent<Player>()._inTutorial))
		{
			_skipCount += 2;
			if (GetNode<Control>("Dialouge").Visible == true)
			{
				if (_skipCount >= 10)
				{
					GetNode<Label>("Dialouge/Warn").Visible = true;
					GetNode<ProgressBar>("Dialouge/Bar").Visible = true;
				}
				if (_skipCount >= 100 && GetNode<Control>("Dialouge").Visible == true)
				{
					GetNode<Control>("Dialouge").Visible = false;
					GetParent().GetParent<fusePathHandler>()._lightsOff = true;
					GetParent<Player>()._currentScreenPos = GetParent<Player>()._defScreenPos;
					_skipCount = 0;
					GetNode<Control>("Tutorial").Visible = true;
				}
			}
			else if (GetNode<Control>("Tutorial").Visible == true)
			{
				if (_skipCount >= 10)
				{
					GetNode<Label>("Tutorial/Warn").Visible = true;
					GetNode<ProgressBar>("Tutorial/Bar").Visible = true;
				}
				if (_skipCount >= 100 && GetNode<Control>("Tutorial").Visible == true)
				{
					GetNode<Control>("Tutorial").Visible = false;
					GetParent<Player>()._inTutorial = false;
				}
			}
		}
		else
		{
			_skipCount = 0;
			GetNode<ProgressBar>("Dialouge/Bar").Visible = false;
			GetNode<ProgressBar>("Tutorial/Bar").Visible = false;
		}
		GetNode<ProgressBar>("Dialouge/Bar").Value = _skipCount;
		GetNode<ProgressBar>("Tutorial/Bar").Value = _skipCount;
	}

	private void _on_return_button_up()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
		GetNode<Control>("Pause").Visible = false;
		GetNode<Control>("Crosshair").Visible = true;
	}

	private void _on_quit_button_up()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}

	private void _on_again_button_up()
    {
		GetTree().ReloadCurrentScene();
    }

	private void _on_controls_button_up()
	{
		GetNode<Sprite2D>("ControlSprite").Visible = true;
		GetNode<Control>("Pause").Visible = false;
	}

	private void _on_back_button_up()
	{
		GetNode<Sprite2D>("ControlSprite").Visible = false;
		GetNode<Control>("Pause").Visible = true;
	}
	
	public async void Type()
	{
		for (_currentCount = 0; _currentCount <= _lineTable[_currentLine].Length; _currentCount++)
		{
			_text.VisibleCharacters = _currentCount;
			await ToSignal(GetTree().CreateTimer(_typingSpeed), SceneTreeTimer.SignalName.Timeout);
		}
		_text.VisibleCharacters = _currentCount;
	}

	public async void Cutscene1()
	{
		MeshInstance3D dark = GetParent().GetNode<MeshInstance3D>("Head/Camera3D/Dark");
		GD.Print(dark);
		for (float i = 0; i < 2; i++)
		{
		  	dark.Visible = true;
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
			dark.Visible = false; 
			await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
		}
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		GetParent().GetParent<fusePathHandler>()._lightsOff = true;
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		_currentLine++;
		Type();
		_text.Text = _lineTable[_currentLine];
		GetNode<Control>("Dialouge").Visible = true;
		_inCutscene = false;
	}

	public async void Cutscene2()
	{
		await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
		_currentLine++;
		Type();
		_text.Text = _lineTable[_currentLine];
		GetNode<Control>("Dialouge").Visible = true;
		_inCutscene = false;
	}
}
