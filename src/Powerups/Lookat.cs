using System.Linq;
using Godot;
using PunchLine.Autoload;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Lookat : Node2D
{
	[Export] private Timer _timer;
	[Export] private AudioStreamPlayer _audioStreamPlayer;

	private AttentionController _attentionController;
	private PlayerCharacter _player;
	
	private PowerupFactory PowerupFactory => GetParent() as PowerupFactory;
	
	public override void _Ready()
	{
		_timer.Timeout += FinishEffect;
		
		var players = GetTree().GetNodesInGroup("Player").Cast<PlayerCharacter>();
		var playerCharacters = players as PlayerCharacter[] ?? players.ToArray();
		var target = playerCharacters.First(p => !Name.ToString().StartsWith(p.PlayerCode));
		
		target.GlobalPosition = new Vector2(-500.0f, -500.0f);
		target.ChangeState(PlayerCharacter.PlayerStates.Chair);
		_attentionController = GetNode<AttentionController>("/root/AttentionController");
		_attentionController.Attention = 50.0f;
		_player = target;
		_timer.Start();
		_audioStreamPlayer.Play();
	}

	private void FinishEffect()
	{
		_player.GlobalPosition = PowerupFactory.GetSpawnPosition(_player.PlayerCode);
		_player.ChangeState(PlayerCharacter.PlayerStates.UnderControl);
		QueueFree();
	}
}