using Godot;
using PunchLine.Autoload;

namespace PunchLine.Hud;

public partial class GameHud : Control
{
    [Export] private ProgressBar _progressBar;
    [Export] private HSlider _hSlider;

    private AttentionController _attentionController;
    
    public override void _PhysicsProcess(double delta)
    {
        _attentionController = GetNode<AttentionController>("/root/AttentionController");

        _progressBar.Value = _attentionController.Attention;
        _hSlider.Value = _attentionController.Attention;
    }
}