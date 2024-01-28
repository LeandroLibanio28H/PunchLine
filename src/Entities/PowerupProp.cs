using Godot;
using PunchLine.Resources;

namespace PunchLine.Entities;

public partial class PowerupProp : Node2D
{
    [Export] private AnimatedSprite2D _throw;
    [Export] private AnimatedSprite2D _icon;
    [Export] private Area2D _sensor;
    [Export] private float _stageLine;
    [Export] private float _moveSpeed;

    private bool _throwing = true;
    public PowerupResource FileResource { get; set; }
    

    public override void _Ready()
    {
        _icon.Hide();
        _throw.Show();
        _throw.SpriteFrames = FileResource.Throw;
        _icon.SpriteFrames = FileResource.Icon;
    }

    public override void _PhysicsProcess(double delta)
    {
        _icon.Play("default");
        _throw.Play("default");
        if (!_throwing) return;
        _throw.RotationDegrees += 360.0f * (float)delta;
        GlobalPosition += Vector2.Up * (float)delta * _moveSpeed;
        if (!(GlobalPosition.Y <= _stageLine)) return;
        _icon.Show();
        _throw.Hide();
        _throwing = false;
    }
}