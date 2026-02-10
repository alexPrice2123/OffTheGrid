using Godot;
using System;

public partial class Disturber : Node3D
{	private int _open;
private float _count = 0;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _open = _rng.RandiRange(1,2);
        if (_open == 2){Visible = false; GetNode<Area3D>("Range").Monitoring = false;}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
       _count += (float)delta;
       if (_count >= 5f)
        {
            _count = 0f;
            if (Visible){Visible = false; GetNode<Area3D>("Range").Monitoring = false;}
            else {Visible = true; GetNode<Area3D>("Range").Monitoring = true;}
        }
    }
}

