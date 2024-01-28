using Godot;
using GodotUtilities.Extensions;
using PunchLine.Systems;

namespace PunchLine;

public partial class MainScene : Node2D
{
    [Export] private Node _scenes;
    [Export] private PackedScene _gameplayScene;
    [Export] private PackedScene _victoryScene;
    [Export] private Camera _camera;
    
    public override void _Ready()
    {
        StartGame();
    }

    private void StartGame()
    {
        _scenes.RemoveAndQueueFreeChildren();
        var game = _gameplayScene.Instantiate();
        _scenes.AddChild(game);
    }

    public void Victory()
    {
        _scenes.RemoveAndQueueFreeChildren();
        var victory = _victoryScene.Instantiate();
        _scenes.AddChild(victory);
    }

    public void ApplyCameraShake(float strength)
    {
        _camera.ApplyShake(strength);
    }
}