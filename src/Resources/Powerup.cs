using Godot;

namespace PunchLine.Resources;

[GlobalClass]
public partial class Powerup : Resource
{
    [Export] private Texture2D _throwingTexture;
    [Export] private Texture2D _icon;
    [Export] private string _group;
}