using Godot;
using System;

public partial class Ui : Control
{
	[Export]
	public string[] _lineTable = { "One", "Two", "Three" };
	private Label _text;
	private float _typingSpeed = 0.025f;
	private int _currentLine = 0;
	private int _currentCount = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _text = GetNode<Label>("Dialouge/Text");
        _text.Text = _lineTable[_currentLine];
		_text.VisibleCharacters = 0;

        Type();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if (Input.IsActionJustPressed("Skip"))
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
		GetNode<Label>("Dialouge/Warn").Visible = _currentCount >= _lineTable[_currentLine].Length;
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
}
