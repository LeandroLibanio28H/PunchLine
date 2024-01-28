using Godot;
using GodotUtilities.Extensions;
using GodotUtilities.Logic;
using PunchLine.Autoload;
using PunchLine.Entities;
using PunchLine.Systems;

namespace PunchLine.Gameplay;

public partial class GameController : Node2D
{
	[Export] private CanvasLayer _mainMenuLayer;
	[Export] private CanvasLayer _hud;
	[Export] private Node2D _curtain;
	[Export] private TomatoesSpawner _tomatoesSpawner;
	[Export] private PowerupSpawner _powerupSpawner;
	[Export] private Node2D _playersNode;
	[Export] private AudioStreamPlayer _audioStreamPlayer;
	
	private DelegateStateMachine _stateMachine;
	private AttentionController _attentionController;

	public override void _Ready()
	{
		_attentionController = GetNode<AttentionController>("/root/AttentionController");
		_attentionController.ResetGame();
		_attentionController.GameOver += () =>
		{
            _stateMachine.ChangeState(GameOverState);
		};
		_stateMachine = new DelegateStateMachine();
		_stateMachine.AddStates(MenuState);
		_stateMachine.AddStates(PausedState);
		_stateMachine.AddStates(WaitingState, EnterWaitingState);
		_stateMachine.AddStates(ContestState, EnterContestState);
		_stateMachine.AddStates(GameOverState, EnterGameOverState);
		
		_stateMachine.SetInitialState(MenuState);
		GetTree().Paused = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		_stateMachine.Update();
	}

	private void MenuState()
	{
		if (Input.IsAnythingPressed())
			_stateMachine.ChangeState(WaitingState);
	}

	private void PausedState()
	{
		
	}

	private void EnterWaitingState()
	{
		var curtainAnim = _curtain.GetFirstNodeOfType<AnimationPlayer>();
		curtainAnim?.Play("open");
		_mainMenuLayer.Hide();
		
		GetTree().Paused = false;
		GetTree().CreateTimer(6.0).Timeout += () =>
		{
			_tomatoesSpawner.Paused = false;
			_powerupSpawner.Paused = false;
			_stateMachine.ChangeState(ContestState);
			_attentionController.ActivateGameplay();
			var players = GetTree().GetNodesInGroup("Player");
			foreach (var player in players)
			{
				if (player is not PlayerCharacter playerCharacter) return;
				playerCharacter.ChangeState(PlayerCharacter.PlayerStates.Default);
			}
		};
	}
	private void WaitingState()
	{
		
	}

	private void EnterContestState()
	{
		_audioStreamPlayer.Play();
	}
	private void ContestState()
	{
		_hud.Show();
	}

	private void EnterGameOverState()
	{
		var curtainAnim = _curtain.GetFirstNodeOfType<AnimationPlayer>();
		curtainAnim!.Play("close");
		curtainAnim!.AnimationFinished += animation =>
		{
			var mainScene = GetTree().CurrentScene as MainScene;
			if (animation != "close") return;
			if (mainScene is null) return;
			
			mainScene.Victory();
		};
		foreach (var player in GetTree().GetNodesInGroup("Player"))
		{
			if (player is PlayerCharacter playerCharacter)
				playerCharacter.ChangeState(PlayerCharacter.PlayerStates.Default);
		}
		_tomatoesSpawner.Paused = true;
		_powerupSpawner.Paused = true;
		_hud.Hide();
	}
	private void GameOverState()
	{
		
	}

	public override void _UnhandledInput(InputEvent @event)
	{
	}
}