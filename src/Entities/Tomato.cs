using Godot;
using GodotUtilities.Extensions;

namespace PunchLine.Entities;

public partial class Tomato : Area2D
{
    [Export] private AnimatedSprite2D _animSprite;
    [Export] private PackedScene _splatScene;
    [Export] private float _minMoveSpeed;
    [Export] private float _maxMoveSpeed;
    [Export] private float _minLifeTime;
    [Export] private float _maxLifeTime;

    private Vector2 _direction;
    private RandomNumberGenerator _randomNumberGenerator;
    private float _moveSpeed;
    private float _lifeTime;

    public override void _Ready()
    {
        AreaEntered += OnHit;
        _randomNumberGenerator = new RandomNumberGenerator();
        var group = _randomNumberGenerator.Randi() % 2 == 0 ? "Player1" : "Player2";

        _animSprite.SpriteFrames =
            GD.Load($"res://scenes/entities/tomato/animations/tomato{_randomNumberGenerator.RandiRange(1, 2)}.tres") as
                SpriteFrames;
        _animSprite.Play("default");

        var players = GetTree().GetNodesInGroup(group);
        if (players is null) return;
        var player = players[0] as Node2D;
        _direction = GlobalPosition.DirectionTo(player!.GlobalPosition);
        _moveSpeed = _randomNumberGenerator.RandfRange(_minMoveSpeed, _maxMoveSpeed);
        _lifeTime = _randomNumberGenerator.RandfRange(_minLifeTime, _maxLifeTime);
        GetTree().CreateTimer(_lifeTime).Timeout += KillTomato;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * _moveSpeed * (float)delta;
    }

    private void KillTomato()
    {
        var splat = _splatScene.Instantiate() as Node2D;
        splat!.GlobalPosition = GlobalPosition;
        this.GetParent().GetSibling<Node2D>("Stage").GetNode("Props").AddChild(splat);
        
        QueueFree();
    }

    private void OnHit(Area2D area2D)
    {
        if (area2D.Owner is not PlayerCharacter playerCharacter) return;
        
        playerCharacter.ChangeState(PlayerCharacter.PlayerStates.UnderControl);
        QueueFree();
    }
}