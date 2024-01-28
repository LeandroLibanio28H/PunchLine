using System.Linq;
using Godot;
using PunchLine.Entities;

namespace PunchLine.Powerups;

public partial class Generator : Node2D
{
    [Export] private Area2D _hitbox;
    [Export] private float _moveSpeed;
    [Export] private float _screenShake;
    [Export] private Texture2D _vfxTexture;
    [Export] private PackedScene _vfxScene;

    private PlayerCharacter _player;
    
    public override void _Ready()
    {
        _hitbox.BodyEntered += FinishEffect;

        var players = GetTree().GetNodesInGroup("Player").Cast<PlayerCharacter>();
        var playerCharacters = players as PlayerCharacter[] ?? players.ToArray();
        _player = playerCharacters.First(p => Name.ToString().StartsWith(p.PlayerCode));
        
        
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


    private void FinishEffect(Node body)
    {
        if (GetTree().GetFirstNodeInGroup("Microphone") is not Node2D microphone)
        {
            QueueFree();
            return;
        }
        
        if (_vfxScene.Instantiate() is Vfx vfx)
        {
            vfx.Sprite.Texture = _vfxTexture;
            vfx.GlobalPosition = GlobalPosition;
            GetParent().AddChild(vfx);
        }

        var main = GetTree().CurrentScene as MainScene;
        main?.ApplyCameraShake(_screenShake);
        microphone.GlobalPosition = new Vector2(_player.GlobalPosition.X, microphone.GlobalPosition.Y);
        QueueFree();
    }

}