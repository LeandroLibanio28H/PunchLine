using Godot;
using GodotUtilities.Extensions;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Anvil : Node2D
{
    [Export] private Area2D _hitbox;
    [Export] private float _moveSpeed;
    [Export] private AudioStreamPlayer _audioStreamPlayer;
    [Export] private float _screenShake;
    [Export] private Texture2D _vfxTexture;
    [Export] private PackedScene _vfxScene;

    private PowerupFactory PowerupFactory => GetParent() as PowerupFactory;
    private bool _active = true;

    public override void _Ready()
    {
        _hitbox.AreaEntered += OnPlayerHit;
        _hitbox.BodyEntered += _ =>
        {
            if (!_active) return;
            _audioStreamPlayer.Play();
            Hide();
        };
        if (GetTree().GetFirstNodeInGroup("Microphone") is not Node2D microphone)
        {
            QueueFree();
            return;
        }
        _audioStreamPlayer.Finished += QueueFree;
        GlobalPosition = new Vector2(microphone.GlobalPosition.X, GlobalPosition.Y);
    }


    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Vector2.Down * _moveSpeed * (float) delta;
    }


    private void OnPlayerHit(Area2D area2D)
    {
        if (area2D.Owner is not PlayerCharacter playerCharacter) return;
        _active = false;
        playerCharacter.GlobalPosition = PowerupFactory.GetSpawnPosition(playerCharacter.PlayerCode);
        playerCharacter.ChangeState(PlayerCharacter.PlayerStates.UnderControl);

        if (_vfxScene.Instantiate() is Vfx vfx)
        {
            vfx.Sprite.Texture = _vfxTexture;
            vfx.GlobalPosition = GlobalPosition;
            GetParent().AddChild(vfx);
        }
        
        _audioStreamPlayer.Play();
        _moveSpeed = 0.0f;
        Hide();
        
        var main = GetTree().CurrentScene as MainScene;
        main?.ApplyCameraShake(_screenShake);
    }
}