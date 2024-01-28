using System.Linq;
using Godot;
using GodotUtilities.Extensions;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Cane : Node2D
{
    [Export] private Area2D _hitboxLeft;
    [Export] private Area2D _hitboxRight;
    [Export] private Sprite2D _rightCane;
    [Export] private Sprite2D _leftCane;
    [Export] private AnimatedSprite2D _rightSprite;
    [Export] private AnimatedSprite2D _leftSprite;
    [Export] private float _moveSpeed;
    [Export] private AudioStreamPlayer _audioStreamPlayer;

    private bool _going = true;
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
        _audioStreamPlayer.Play();

        if (positionTest < 0)
        {
            _hitboxRight.DisableArea();
            _rightCane.Visible = false;
        }
        else
        {
            _hitboxLeft.DisableArea();
            _leftCane.Visible = false;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (_going)
        {
            _leftCane.GlobalPosition += Vector2.Right * (float)delta * _moveSpeed;
            _rightCane.GlobalPosition += Vector2.Left * (float)delta * _moveSpeed;

            if (!(_leftCane.GlobalPosition.X >= 1280.0f)) return;
            _going = false;
            _audioStreamPlayer.Stop();
            _audioStreamPlayer.Play();
        }
        else
        {
            _leftCane.GlobalPosition += Vector2.Left * (float)delta * _moveSpeed;
            _rightCane.GlobalPosition += Vector2.Right * (float)delta * _moveSpeed;

            if (!(_leftCane.GlobalPosition.X <= 0.0f)) return;
            if (_player is not null)
            {
                _player.GlobalPosition = PowerupFactory.GetSpawnPosition(_player.PlayerCode);
                _player.ChangeState(PlayerCharacter.PlayerStates.UnderControl);
            }
            QueueFree();
        }
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
        _rightSprite.Play("walk");

        _going = false;
        _audioStreamPlayer.Play();
    }
}