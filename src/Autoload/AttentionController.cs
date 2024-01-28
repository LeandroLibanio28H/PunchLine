using System;
using Godot;

namespace PunchLine.Autoload;

public partial class AttentionController : Node
{
    [Signal]
    public delegate void GameOverEventHandler();

    [Export] private Timer _awayTimer;
    [Export] private AnimatedSprite2D _counter;


    private bool _isGameActive;
    private bool _isAnyoneAtMic;


    public float Attention { get; set; } = 50.0f;
    private const float MaxAttention = 100.0f;


    public void ResetGame()
    {
        _isGameActive = false;
        _isAnyoneAtMic = false;
        Attention = 50.0f;
        _counter.Visible = false;
        _counter.Stop();
        _awayTimer.Stop();
    }

    public void ActivateGameplay()
    {
        _isGameActive = true;
    }
    
    public override void _Ready()
    {
        _awayTimer.Timeout += () =>
        {
            if (!_isGameActive) return;
            _isGameActive = false;
            _counter.Visible = false;
            _counter.Stop();
            EmitSignal(SignalName.GameOver);
        };
    }


    public void DrainPoints()
    {
        _isAnyoneAtMic = false;
    }

    public void StopDrainingPoints()
    {
        _isAnyoneAtMic = true;
        _awayTimer.Stop();
        _counter.Visible = false;
        _counter.Stop();
    }


    public override void _PhysicsProcess(double delta)
    {
        if (_isAnyoneAtMic || !_isGameActive) return;

        Attention = Mathf.MoveToward(Attention, 50.0f, 3 * (float)delta);
        if (!(Math.Abs(Attention - 50.0f) < 0.001f) || !_awayTimer.IsStopped()) return;
        Attention = 50.0f;
        _awayTimer.Start();
        _counter.Stop();
        _counter.Visible = true;
        _counter.Play("default");
    }

    public void Score(float amount)
    {
        Attention += amount;

        if (Attention is > 0.0f and < MaxAttention) return;
        _isGameActive = false;
        _counter.Visible = false;
        _counter.Stop();
        EmitSignal(SignalName.GameOver);
    }
}