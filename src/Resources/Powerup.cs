using Godot;

namespace PunchLine.Resources;

[GlobalClass]
public partial class Powerup : Resource
{
    [Export] private SpriteFrames _throw;
    [Export] private SpriteFrames _icon;
    [Export] private string _group;
}