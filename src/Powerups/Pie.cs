using System.Linq;
using Godot;
using GodotUtilities.Extensions;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Pie : Node2D
{
    [Export] private Area2D _hitboxLeft;
    [Export] private Area2D _hitboxRight;
    [Export] private AnimatedSprite2D _rightPie;
    [Export] private AnimatedSprite2D _leftPie;
    [Export] private AnimatedSprite2D _rightSprite;
    [Export] private AnimatedSprite2D _leftSprite;
    [Export] private float _moveSpeed;
    [Export] private AudioStreamPlayer _audioStreamPlayer;
    [Export] private Sprite2D _vfxRight;
    [Export] private Sprite2D _vfxLeft;
    
    private PlayerCharacter _player;
    
    private PowerupFactory PowerupFactory => GetParent() as PowerupFactory;


    public override void _Ready()
    {
        _hitboxLeft.AreaEntered += OnPlayerHit;
        _hitboxRight.AreaEntered += OnPlayerHit;

        var players = GetTree().GetNodesInGroup("Player").Cast<PlayerCharacter>();
        var playerCharacters = players as PlayerCharacter[] ?? players.ToArray();
        var caster = playerCharacters.First(p => Name.ToString().StartsWith(p.PlayerCode));
        var target = playerCharacters.First(p => !Name.ToString().StartsWith(p.PlayerCode));

        var positionTest = target.GlobalPosition.X - caster.GlobalPosition.X;

        if (positionTest > 0)
        {
            _hitboxRight.DisableArea();
            _rightPie.Visible = false;
        }
        else
        {
            _hitboxLeft.DisableArea();
            _leftPie.Visible = false;
        }
        _audioStreamPlayer.Play();
    }


    public override void _PhysicsProcess(double delta)
    {
        _leftPie.GlobalPosition += Vector2.Right * (float)delta * _moveSpeed;
        _rightPie.GlobalPosition += Vector2.Left * (float)delta * _moveSpeed;

        if (!(_leftPie.GlobalPosition.X >= 1280.0f)) return;
        if (_player is not null)
        {
            _player.GlobalPosition = PowerupFactory.GetSpawnPosition(_player.PlayerCode);
            _player.ChangeState(PlayerCharacter.PlayerStates.UnderControl);
        }
        QueueFree();
    }


    private void OnPlayerHit(Area2D area2D)
    {
        if (_player is not null) return;
        if (area2D.Owner is not PlayerCharacter playerCharacter) return;

        _leftSprite.SpriteFrames = playerCharacter.AnimSprite.SpriteFrames;
        _rightSprite.SpriteFrames = playerCharacter.AnimSprite.SpriteFrames;

        _player = playerCharacter;
        playerCharacter.GlobalPosition = new Vector2(-500.0f, -500.0f);
        playerCharacter.ChangeState(PlayerCharacter.PlayerStates.Chair);
        
        _leftSprite.Play("walk");
        _vfxLeft.Show();
        _vfxRight.Show();
        _rightSprite.Play("walk");
    }
}