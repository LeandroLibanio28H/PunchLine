using Godot;
using PunchLine.Autoload;
using PunchLine.Systems;

namespace PunchLine.Hud;

public partial class Victory : CanvasLayer
{
    [Export] private Control _uiNode;
    [Export] private AnimatedSprite2D _player1;
    [Export] private AnimatedSprite2D _player2;
    [Export] private TomatoesSpawner _tomatoes;
    [Export] private TextureRect _winTexture;
    [Export] private TextureRect _loseTexture;

    private AttentionController _attentionController;


    public override void _Ready()
    {
        _attentionController = GetNode<AttentionController>("/root/AttentionController");

        switch (_attentionController.Attention)
        {
            case >= 100.0f:
                _player1.Show();
                _player2.Hide();
                _tomatoes.QueueFree();
                break;
            case <= 0.0f:
                _player1.Hide();
                _player2.Show();
                _tomatoes.QueueFree();
                break;
            default:
                _player1.Show();
                _player2.Show();
                _player1.Play("crouch");
                _player2.Play("crouch");
                _winTexture.Hide();
                _loseTexture.Show();
                break;
        }
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsPressed() && _uiNode.Visible)
        {
            GetTree().ReloadCurrentScene();
        }
    }
}