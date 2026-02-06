using Godot;
using System;

public partial class Minion : Node3D
{
	private GpuParticles3D _eyeOpen;
	private GpuParticles3D _eyeClosed;
	private int _count = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _eyeOpen = GetNode<GpuParticles3D>("EyeOpen");
		_eyeClosed = GetNode<GpuParticles3D>("EyeClosed");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        _count += 1;
		if (_count > 500)
        {
            Eyes(true);
        }
    }

	private void Eyes(bool open)
    {
        _eyeOpen.Visible = open;
		_eyeClosed.Visible = !open;
    }
}
