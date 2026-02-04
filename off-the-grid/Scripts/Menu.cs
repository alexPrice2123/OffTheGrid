using Godot;
using System;

public partial class Menu : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_play_button_up()
    {
        GetTree().ChangeSceneToFile("res://Scenes/world.tscn");
    }

	private void _on_quit_button_up()
    {
        GetTree().Quit();
    }

	private void _on_controls_button_up()
    {
        GetNode<Sprite2D>("ControlSprite").Visible = true;
		GetNode<Control>("MenuUI").Visible = false;
    }

	private void _on_back_button_up()
    {
        GetNode<Sprite2D>("ControlSprite").Visible = false;
		GetNode<Control>("MenuUI").Visible = true;
    }
}
