using System.Linq;
using Godot;
using PunchLine.Autoload;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Powerups;

public partial class Pickle : Node2D
{
	private PowerupFactory PowerupFactory => GetParent() as PowerupFactory;
	private PlayerCharacter _player;
	
	public override void _Ready()
	{
		var players = GetTree().GetNodesInGroup("Player").Cast<PlayerCharacter>();
		var playerCharacters = players as PlayerCharacter[] ?? players.ToArray();
		var caster = playerCharacters.First(p => Name.ToString().StartsWith(p.PlayerCode));
		var target = playerCharacters.First(p => !Name.ToString().StartsWith(p.PlayerCode));

		target.GlobalPosition = PowerupFactory.GetSpawnPosition(target.PlayerCode);
		target.ChangeState(PlayerCharacter.PlayerStates.Chair);
		
		caster.ChangeState(PlayerCharacter.PlayerStates.Pickle);
		_player = caster;
		caster.AnimPlayer.AnimationFinished += PickeFinished;
		
		var attentionController = GetNode<AttentionController>("/root/AttentionController");
		attentionController.StopDrainingPoints();
	}

	private void PickeFinished(StringName animName)
	{
		if (animName != "pickle") return;
		var attentionController = GetNode<AttentionController>("/root/AttentionController");
			
		attentionController.Score(_player.PlayerCode == "p1_" ? 100.0f : -100.0f);
	}
}