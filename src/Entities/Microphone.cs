using Godot;
using Godot.Collections;

namespace PunchLine.Entities;

public partial class Microphone : Node2D
{
    [Export] private Area2D _sensor;
    [Export] private Area2D _tomatoKiller;
    [Export] private TextureRect _buttonPanel;
    private bool _active = true;

    private string[] _quickTimeEventActions = new[] { "special_a", "special_b" };

    private PlayerCharacter _currentPlayerCharacter;
    private string _currentActionListening = string.Empty;
    private Dictionary<string, Texture2D> _keyTextures;

    private RandomNumberGenerator _randomNumberGenerator;

    public override void _Ready()
    {
        _keyTextures = new Dictionary<string, Texture2D>();
        foreach (var key in _quickTimeEventActions)
        {
            _keyTextures.Add("p1_" + key, 
                GD.Load($"res://scenes/entities/microphone/textures/p1_{key}.png") as Texture2D);
            _keyTextures.Add("p2_" + key, 
                GD.Load($"res://scenes/entities/microphone/textures/p2_{key}.png") as Texture2D);
        }
        
        _sensor.AreaEntered += OnSensorEntered;
        _sensor.AreaExited += OnSensorExited;
        _tomatoKiller.AreaEntered += OnTomatoKillerEntered;
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
        SetProcessUnhandledInput(false);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_currentPlayerCharacter is null || !@event.IsPressed()) return;
        if (@event.IsAction(_currentActionListening))
        {
            // TODO: Score
            
            ChangeCurrentActionListening();
        }
        else if (@event.IsAction(_currentPlayerCharacter.PlayerCode + "special_a") ||
                 @event.IsAction(_currentPlayerCharacter.PlayerCode + "special_b"))
        {
            // TODO: Failed
            
            StopQuickTimeEvent();
        }
    }

    private void OnSensorEntered(Area2D area2D)
    {
        if (!_active || area2D.Owner is not PlayerCharacter playerCharacter) return;
        
        _currentPlayerCharacter = playerCharacter;
        _active = false;
        
        StartQuickTimeEvent();
    }
    
    private void OnSensorExited(Area2D area2D)
    {
        if (_active || area2D.Owner is not PlayerCharacter playerCharacter) return;
        if (_currentPlayerCharacter != playerCharacter) return;
        
        _currentPlayerCharacter = null;
        _active = true;
        
        StopQuickTimeEvent();
    }

    private void OnTomatoKillerEntered(Area2D area2D)
    {
        if (area2D is not Tomato tomato) return;
        
        tomato.QueueFree();
    }

    private void StartQuickTimeEvent()
    {
        SetProcessUnhandledInput(true);
        ChangeCurrentActionListening();
    }

    private void StopQuickTimeEvent()
    {
        SetProcessUnhandledInput(false);
        _currentActionListening = string.Empty;
        _buttonPanel.Texture = null;
    }

    private void ChangeCurrentActionListening()
    {
        _currentActionListening = _currentPlayerCharacter.PlayerCode +
            _quickTimeEventActions[_randomNumberGenerator.RandiRange(0, _quickTimeEventActions.Length - 1)];

        _buttonPanel.Texture = _keyTextures[_currentActionListening];
    }
}