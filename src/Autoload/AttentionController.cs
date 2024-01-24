using Godot;

namespace PunchLine.Autoload;

public partial class AttentionController : Node
{
    [Signal]
    public delegate void GameOverEventHandler(float attention);


    public float Attention { get; set; }
    private readonly float _maxAttention;


    public AttentionController(float maxAttention)
    {
        _maxAttention = maxAttention;
        Attention = maxAttention / 2.0f;
    }


    public void Score(float amount)
    {
        Attention += amount;

        if (Attention <= 0.0f || Attention >= _maxAttention)
            EmitSignal(SignalName.GameOver, Attention);
    }
}