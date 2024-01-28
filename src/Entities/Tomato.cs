using Godot;

namespace PunchLine.Entities;

public partial class Tomato : Area2D
{
    [Export] private AnimatedSprite2D _animSprite;
    [Export] private PackedScene _splatScene;
    [Export] private Area2D _killerSensor;
    [Export] private float _minMoveSpeed;
    [Export] private float _maxMoveSpeed;
    [Export] private float _minLifeTime;
    [Export] private float _maxLifeTime;
    [Export] private float _screenShakeStrength;
    [Export] private Texture2D _vfxTexture;
    [Export] private PackedScene _vfxScene;

    private Vector2 _direction;
    private RandomNumberGenerator _randomNumberGenerator;
    private float _moveSpeed;
    private float _lifeTime;
    private bool _active = true;
    public string TargetGroup { get; set; }

    public override void _Ready()
    {
        AreaEntered += OnHit;
        _killerSensor.AreaEntered += _ =>
        {
            QueueFree();
        };
        _randomNumberGenerator = new RandomNumberGenerator();
        var group = _randomNumberGenerator.Randi() % 2 == 0 ? "Player1" : "Player2";
        if (!string.IsNullOrWhiteSpace(TargetGroup))
            group = TargetGroup;

        _animSprite.SpriteFrames =
            GD.Load($"res://scenes/entities/tomato/animations/tomato{_randomNumberGenerator.RandiRange(1, 2)}.tres") as
                SpriteFrames;
        _animSprite.Play("default");

        var targets = GetTree().GetNodesInGroup(group);
        if (targets is null) return;
        var target = targets[0] as Node2D;
        _direction = GlobalPosition.DirectionTo(target!.GlobalPosition);
        _moveSpeed = _randomNumberGenerator.RandfRange(_minMoveSpeed, _maxMoveSpeed);
        _lifeTime = _randomNumberGenerator.RandfRange(_minLifeTime, _maxLifeTime);
        GetTree().CreateTimer(_lifeTime).Timeout += KillTomato;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * _moveSpeed * (float)delta;
    }
    
    public void KillTomato()
    {
        _active = false;
        var splat = _splatScene.Instantiate() as Node2D;
        splat!.GlobalPosition = GlobalPosition;
        GetTree().GetFirstNodeInGroup("Props")!.AddChild(splat);
        
        QueueFree();
    }

    private void OnHit(Area2D area2D)
    {
        if (!_active) return;
        _active = false;
        if (area2D.Owner is not PlayerCharacter playerCharacter) return;
        if (playerCharacter.Pickle) return;
        
        playerCharacter.ChangeState(PlayerCharacter.PlayerStates.UnderControl);
        var main = GetTree().CurrentScene as MainScene;
        main?.ApplyCameraShake(_screenShakeStrength);
        if (_vfxScene.Instantiate() is Vfx vfx)
        {
            vfx.Sprite.Texture = _vfxTexture;
            vfx.Position = Position;
            vfx.Scale = new Vector2(0.5f, 0.5f);
            GetParent().AddChild(vfx);
        }
        QueueFree();
    }
}