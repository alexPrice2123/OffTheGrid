using Godot;
using System;

public partial class Disturbor : Node3D
{	private int _open;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _open = _rng.RandiRange(1,2);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
       
    }
}
