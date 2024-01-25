using Godot;

namespace PunchLine.Systems;

public partial class TomatoesSpawner : Path2D
{
    [Export] private PackedScene _tomatoScene;
    [Export] private PathFollow2D _spawnPoint;
    [Export] private Timer _timer;

    private RandomNumberGenerator _randomNumberGenerator;

    public override void _Ready()
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
        
        _timer.Timeout += SpawnTomato;
    }

    private void SpawnTomato()
    {
        _spawnPoint.ProgressRatio = _randomNumberGenerator.Randf();
        var tomato = _tomatoScene.Instantiate() as Node2D;
        tomato!.GlobalPosition = _spawnPoint.GlobalPosition;
        AddChild(tomato);
    }
}