using Godot;
using System;

public partial class Ui : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
}
