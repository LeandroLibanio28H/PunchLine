using Godot;

namespace GodotUtilities.Extensions;

public static class Node2DExtension
{
    public static Vector2 GetMouseDirection(this Node2D node) =>
        (node.GetGlobalMousePosition() - node.GlobalPosition).Normalized();

    public static Vector2 GetLocalMouseDirection(this Node2D node) =>
        node.GetLocalMousePosition().Normalized();
}