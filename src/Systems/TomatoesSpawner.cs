using System;
using Godot;
using PunchLine.Entities;

namespace PunchLine.Systems;

public partial class TomatoesSpawner : Path2D
{
    [Export] private PackedScene _tomatoScene;
    [Export] private PathFollow2D _spawnPoint;
    [Export] private Timer _timer;
    [Export] private string _group = string.Empty;
    [Export] private double _spawnTime;

    [Export] public bool Paused { get; set; } = true;
    private RandomNumberGenerator _randomNumberGenerator;

    public override void _Ready()
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();

        _timer.WaitTime = _spawnTime;
        _timer.Timeout += SpawnTomato;
    }

    private void SpawnTomato()
    {
        if (Paused) return;
        _spawnPoint.ProgressRatio = _randomNumberGenerator.Randf();
        var tomato = _tomatoScene.Instantiate() as Tomato;
        tomato!.GlobalPosition = _spawnPoint.GlobalPosition;
        if (!string.IsNullOrWhiteSpace(_group))
            tomato.TargetGroup = _group;
        AddChild(tomato);
    }
}