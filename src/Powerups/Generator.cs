using System.Linq;
using Godot;
using PunchLine.Entities;

namespace PunchLine.Powerups;

public partial class Generator : Node2D
{
    [Export] private Area2D _hitbox;
    [Export] private float _moveSpeed;

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

        microphone.GlobalPosition = new Vector2(_player.GlobalPosition.X, microphone.GlobalPosition.Y);
        QueueFree();
    }

}