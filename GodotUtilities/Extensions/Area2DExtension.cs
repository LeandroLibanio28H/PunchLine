using Godot;

namespace GodotUtilities.Extensions;

public static class Area2DExtension
{
    public static void DisableArea(this Area2D area2D)
    {
        area2D.Monitorable = false;
        area2D.Monitoring = false;
    }

    public static void EnableArea(this Area2D area2D)
    {
        area2D.Monitorable = true;
        area2D.Monitoring = true;
    }
}