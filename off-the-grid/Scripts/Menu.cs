using Godot;
using System;

public partial class Menu : Node3D
{
	private ShaderMaterial _transitionMat;
	private AudioStreamPlayer _sound;
	private int _trans = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		_transitionMat = GetNode<ColorRect>("Transition").Material as ShaderMaterial;
		_sound = GetNode<AudioStreamPlayer>("Music");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_trans != 2)
		{
			_transitionMat.SetShaderParameter("fade", Mathf.Lerp((float)_transitionMat.GetShaderParameter("fade"), _trans, delta * 2));
			_sound.VolumeDb = Mathf.Lerp(_sound.VolumeDb, _trans*-20, (float)delta * 2);
		}
    }

	private async void _on_play_button_up()
	{
		_trans = 1;
		await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile("res://Scenes/Map.tscn");
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
