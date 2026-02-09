using Godot;
using System;

public partial class Minion : Node3D
{
    [Export]
    public float _maxY = 45;
    [Export]
    public float _minY = -45;
	private GpuParticles3D _eyeOpen;
    private GpuParticles3D _eyeClosed;
    private Node3D _vision;
    private bool _direction = false;
    private int _count = 1;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _eyeOpen = GetNode<GpuParticles3D>("EyeOpen");
        _eyeClosed = GetNode<GpuParticles3D>("EyeClosed");
        _vision = GetNode<Node3D>("EyeOpen/Sight");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        _count += 1;
        if (_count == 250)
        {
            Eyes(true);
        }
        if (_count == 1000)
        {
            Eyes(false);
            _count = _rng.RandiRange(-50,50);
        }
        if (_eyeOpen.Visible)
        {
            if (_direction) { _vision.RotateY(0.01f); }
            else { _vision.RotateY(-0.01f); }
        }
        float y = Mathf.RadToDeg(_vision.Rotation.Y);
        if (y > _maxY && _direction) { _direction = false; }
        if (y < _minY && !_direction){ _direction = true; }
    }

    private void Eyes(bool open)
    {
        _eyeOpen.Visible = open;
        _eyeClosed.Visible = !open;
    }
    
    private void _on_vision_body_entered(Node3D body)
    {
        if (body is Player && _eyeOpen.Visible)
        {
            Eyes(false);
            GetNode<GpuParticles3D>("Found").Emitting = true;
            GetParent().GetNode<Monster>("Monster")._goalPos = body.GlobalPosition;
            GetNode<AudioStreamPlayer>("Scream").Play();
        }
    }
}
