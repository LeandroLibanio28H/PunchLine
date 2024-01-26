using Godot;
using Godot.Collections;
using GodotUtilities.Extensions;
using PunchLine.Autoload;
using PunchLine.Systems;

namespace PunchLine.Entities;

public partial class Microphone : Node2D
{
    [Export] private Area2D _sensor;
    [Export] private Area2D _tomatoKiller;
    [Export] private TextureRect _buttonPanel;
    [Export] private TextureRect _borderPanel;
    [Export] private TomatoesSpawner _tomatoesSpawner;
    private bool _active = true;

    private string[] _quickTimeEventActions = new[] { "special_a", "special_b" };

    private PlayerCharacter _currentPlayerCharacter;
    private string _currentActionListening = string.Empty;
    private Dictionary<string, Texture2D> _keyTextures;
    private AttentionController _attentionController;

    private RandomNumberGenerator _randomNumberGenerator;

    public override void _Ready()
    {
        _attentionController = GetNode<AttentionController>("/root/AttentionController");
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
        _borderPanel.Hide();
        SetProcessUnhandledInput(false);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_currentPlayerCharacter is null || !@event.IsPressed()) return;
        if (@event.IsAction(_currentActionListening))
        {
            _attentionController.Score(10.0f * (_currentPlayerCharacter.PlayerCode == "p1_" ? 1.0f : -1.0f));
            
            ChangeCurrentActionListening();
        }
        else if (@event.IsAction(_currentPlayerCharacter.PlayerCode + "special_a") ||
                 @event.IsAction(_currentPlayerCharacter.PlayerCode + "special_b"))
        {
            RemoveShield();
            
            StopQuickTimeEvent();
        }
    }

    private void OnSensorEntered(Area2D area2D)
    {
        if (!_active || area2D.Owner is not PlayerCharacter playerCharacter) return;
        
        _currentPlayerCharacter = playerCharacter;
        _currentPlayerCharacter.ChangeState(PlayerCharacter.PlayerStates.Microphone);
        _active = false;
        
        _attentionController.StopDrainingPoints();
        StartQuickTimeEvent();
    }
    
    private void OnSensorExited(Area2D area2D)
    {
        if (_active || area2D.Owner is not PlayerCharacter playerCharacter) return;
        if (_currentPlayerCharacter != playerCharacter) return;
        
        if (_currentPlayerCharacter.GetState() == PlayerCharacter.PlayerStates.Microphone)
            _currentPlayerCharacter.ChangeState(PlayerCharacter.PlayerStates.Default);
        
        _currentPlayerCharacter = null;
        _active = true;
        
        _attentionController.DrainPoints();
        StopQuickTimeEvent();
    }

    private static void OnTomatoKillerEntered(Area2D area2D)
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
        _borderPanel.Hide();
    }

    private void ChangeCurrentActionListening()
    {
        _currentActionListening = _currentPlayerCharacter.PlayerCode +
            _quickTimeEventActions[_randomNumberGenerator.RandiRange(0, _quickTimeEventActions.Length - 1)];

        _borderPanel.Show();
        _buttonPanel.Texture = _keyTextures[_currentActionListening];
    }

    public void RemoveShield()
    {
        _tomatoKiller.DisableArea();
        _sensor.DisableArea();
        _tomatoesSpawner.Paused = false;
        GetTree().CreateTimer(3.5).Timeout += () =>
        {
            _tomatoKiller.EnableArea();
            _sensor.EnableArea();
            _tomatoesSpawner.Paused = true;
        };
    }
}