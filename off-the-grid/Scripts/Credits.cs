using Godot;
using System;

public partial class Credits : Control
{
    private bool _rolling = true;
    public async override void _Ready()
	{
        while (GetNode<Label>("Text").Position.Y > -362.0)
        {
           await ToSignal(GetTree().CreateTimer(0.01f), SceneTreeTimer.SignalName.Timeout); 
        }
        _rolling = false;
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        for (int i = 0; i < 20; i++)
        {
            GetNode<ColorRect>("Block").Color += new Color(0, 0, 0, 0.05f);
            await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
        }
        await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
        for (int i = 0; i < 20; i++)
        {
            GetNode<Label>("Text").Modulate -= new Color(0, 0, 0, 0.05f);
            await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
        }
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
    }
    public override void _Process(double delta)
    {
        if (_rolling)
        {
            GetNode<Label>("Text").Position += new Vector2(0f, -0.5f);
        }
        
    }
}
