using Godot;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Anvil : Node2D
{
    [Export] private Area2D _hitbox;
    [Export] private float _moveSpeed;
    [Export] private AudioStreamPlayer _audioStreamPlayer;
    [Export] private float _screenShake;

    private PowerupFactory PowerupFactory => GetParent() as PowerupFactory;
    private bool _active = true;

    public override void _Ready()
    {
        _hitbox.AreaEntered += OnPlayerHit;
        _audioStreamPlayer.Play();
        _hitbox.BodyEntered += _ =>
        {
            if (_active)
            {
                QueueFree();
            }
        };
        if (GetTree().GetFirstNodeInGroup("Microphone") is not Node2D microphone)
        {
            QueueFree();
            return;
        }
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
        var main = GetTree().CurrentScene as MainScene;
        main?.ApplyCameraShake(_screenShake);
        QueueFree();
    }
}