using System;
using System.Linq;
using Godot;
using PunchLine.Entities;

namespace PunchLine.Systems;

public partial class PowerupFactory : Marker2D
{
    [Export] private PackedScene _powerupScene;
    
    
    public void ActivatePowerup(string playerCode)
    {
        if (_powerupScene.Instantiate() is not Node2D scene) return;
        scene.GlobalPosition = GlobalPosition;
        scene.Name = playerCode + new RandomNumberGenerator().Randi();
        AddChild(scene);
    }

    public Vector2 GetSpawnPosition(string playerCode)
    {
        var chairs = GetTree().GetNodesInGroup("Chair").Cast<Node2D>().ToList();
        
        if (chairs.Count > 0)
        {
            if (GetTree().GetFirstNodeInGroup("Microphone") is Microphone microphone)
            {
                if (Math.Abs(microphone.GlobalPosition.X - 640.0f) < 0.001f)
                {
                    var chair =
                        chairs.FirstOrDefault(c => c.IsInGroup(playerCode == "p1_" ? "Left" : "Right"));

                    if (chair != null) return chair.GlobalPosition;
                }
                else
                {
                    var chair = chairs.OrderByDescending(c => c.GlobalPosition.DistanceTo(microphone.GlobalPosition))
                        .First();
                    return chair.GlobalPosition;
                }
            }
        }
        GetTree().Quit();
        return Vector2.Zero;
    }
}