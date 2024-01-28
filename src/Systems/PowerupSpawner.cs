using Godot;
using Godot.Collections;
using PunchLine.Entities;
using PunchLine.Resources;

namespace PunchLine.Systems;

public partial class PowerupSpawner : Path2D
{
    [Export] private PackedScene _powerupScene;
    [Export] private PathFollow2D _spawnPoint;
    [Export] private Timer _timer;
    [Export] private Array<PowerupResource> _powerupResources;
    [Export] private PowerupResource _generator;
    [Export] private PowerupResource _pickle;

    public bool Paused = true;
    
    private RandomNumberGenerator _randomNumberGenerator;
    
    public override void _Ready()
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();

        _timer.Timeout += SpawnPowerup;
        _timer.Start();
    }

    private void SpawnPowerup()
    {
        if (Paused || GetTree().GetNodesInGroup("Powerup").Count >= 3) return;
        _spawnPoint.ProgressRatio = _randomNumberGenerator.Randf();

        if (_randomNumberGenerator.RandiRange(0, 100) <= 10) return;
        if (_powerupScene.Instantiate() is not PowerupProp powerup) return;

        var value = _randomNumberGenerator.RandiRange(0, 1000000);
        PowerupResource res = null;
        switch (value)
        {
            case 42:
                //TODO: Picles
                res = _pickle;
                break;
            case > 1000 and < 5000:
                //TODO: Improbability
                res = _generator;
                break;
            default:
                res = _powerupResources.PickRandom();
                break;
        }
        
        powerup.FileResource = res;
        powerup.GlobalPosition = _spawnPoint.GlobalPosition;
        AddChild(powerup);
    }
}