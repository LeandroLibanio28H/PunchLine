using Godot;

namespace PunchLine.Systems;

public partial class Camera : Camera2D
{
    private float _shakeDecayRate = 5.0f;

    private RandomNumberGenerator _randomNumberGenerator;

    private float _shakeStrength;


    public override void _Ready()
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
    }

    public override void _PhysicsProcess(double delta)
    {
        _shakeStrength = Mathf.Lerp(_shakeStrength, 0.0f, _shakeDecayRate * (float)delta);

        Offset = GetRandomOffset();
    }

    private Vector2 GetRandomOffset()
    {
        return new Vector2(_randomNumberGenerator.RandfRange(-_shakeStrength, _shakeStrength),
            _randomNumberGenerator.RandfRange(-_shakeStrength, _shakeStrength));
    }

    public void ApplyShake(float strength)
    {
        _shakeStrength = strength;
    }
}