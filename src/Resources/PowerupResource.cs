using Godot;

namespace PunchLine.Resources;

[GlobalClass]
public partial class PowerupResource : Resource
{
    [Export] public SpriteFrames Throw;
    [Export] public SpriteFrames Icon;
    [Export] public string Group;
}