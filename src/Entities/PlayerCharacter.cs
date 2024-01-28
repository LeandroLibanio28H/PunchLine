using System;
using Godot;
using Godot.Collections;
using GodotUtilities.Extensions;
using GodotUtilities.Logic;
using PunchLine.Resources;
using PunchLine.Systems;

namespace PunchLine.Entities;

public partial class PlayerCharacter : CharacterBody2D
{
	public enum PlayerStates
	{
		Default,
		Microphone,
		UnderControl,
		Chair,
		Pickle
	}
	
	[Export] private Area2D _hitBox;
	[Export] public AnimatedSprite2D AnimSprite;
	[Export] public AnimationPlayer AnimPlayer;
	[Export] private AnimatedSprite2D _powerupIcon;
	[Export] private Area2D _microphoneSensor;
	[Export] private Area2D _powerupSensor;
	[Export] private Timer _controlTimer;
	[Export] private float _moveSpeed;
	[Export] private float _jumpStrength;
	[Export] private Area2D _tomatoKiller;
	[Export] private SpriteFrames _player1Sprite;
	[Export] private SpriteFrames _player2Sprite;
	[Export] private AudioStreamPlayer2D _audioStreamPlayer;
	
	[Export] private Array<AudioStream> _p1AudioStart;
	[Export] private Array<AudioStream> _p2AudioStart;
	
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public string PlayerCode { get; private set; }
	private string[] _playerCharacters = new[] { "casado", "hilario" };

	private RandomNumberGenerator _randomNumberGenerator;

	#region Flags

	private bool _canJump;
	public bool Pickle;
	private PowerupResource CurrentPowerup { get; set; }

	#endregion

	private DelegateStateMachine _stateMachine;
	
	
	public override void _Ready()
	{
		CurrentPowerup = null;
		_controlTimer.Timeout += () =>
		{
			_stateMachine.ChangeState(DefaultState);
		};
		_randomNumberGenerator = new RandomNumberGenerator();
		_randomNumberGenerator.Randomize();
		_stateMachine = new DelegateStateMachine();
		_stateMachine.AddStates(DefaultState, EnterDefaultState);
		_stateMachine.AddStates(JumpState, EnterJumpState, LeaveJumpState);
		_stateMachine.AddStates(MicrophoneState);
		_stateMachine.AddStates(CrouchedState, EnterCrouchedState, LeaveCrouchedState);
		_stateMachine.AddStates(UnderControlState, EnterUnderControlState, LeaveUnderControlState);
		_stateMachine.AddStates(SitDownState);
		_stateMachine.AddStates(PickleState, EnterPickleState);
		
		_stateMachine.SetInitialState(SitDownState);
		
		PlayerCode = IsInGroup("Player1") ? "p1_" : "p2_";
		AnimSprite.SpriteFrames = PlayerCode == "p1_" ? _player1Sprite : _player2Sprite;
		AnimSprite.FlipH = IsInGroup("Player2");

		_powerupSensor.AreaEntered += area =>
		{
			switch (area.Owner)
			{
				case PowerupProp powerup when CurrentPowerup is null:
					CurrentPowerup = powerup.FileResource;
					_powerupIcon.SpriteFrames = CurrentPowerup.Icon;
					_powerupIcon.AddToGroup("Powerup");
					powerup.QueueFree();
					break;
				case PlayerCharacter { CurrentPowerup: not null } playerCharacter when CurrentPowerup is null:
					CurrentPowerup = playerCharacter.CurrentPowerup;
					_powerupIcon.SpriteFrames = CurrentPowerup.Icon;
					_powerupIcon.AddToGroup("Powerup");
					playerCharacter.CurrentPowerup = null;
					playerCharacter._powerupIcon.RemoveFromGroup("Powerup");
					break;
			}
		};
	}
	
	public override void _PhysicsProcess(double delta)
	{
		_powerupIcon.Play("default");
		if (CurrentPowerup is not null)
		{
			_powerupIcon.Show();
			_powerupIcon.AddToGroup("Powerup");
		}
		else
		{
			_powerupIcon.Hide();
			_powerupIcon.RemoveFromGroup("Powerup");
		}
		var velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity.Y += _gravity * (float)delta;
		}
		
		var direction = Input.GetActionStrength(PlayerCode + "move_right") - 
		                Input.GetActionStrength(PlayerCode + "move_left");
		if (direction != 0.0f)
			velocity.X = direction * _moveSpeed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _moveSpeed);

		Velocity = velocity;
		
		AnimSprite.FlipH = Velocity.X switch
		{
			< 0.0f => true,
			> 0.0f => false,
			_ => AnimSprite.FlipH
		};
		
		_stateMachine.Update();
	}

	public override void _Notification(int what)
	{
		if (what != NotificationPredelete) return;
		_stateMachine.Dispose();
		SetPhysicsProcess(false);
		SetProcessUnhandledInput(false);
	}

	public void PlayJoke()
	{
		if (_audioStreamPlayer.Playing) return;
		var audio = PlayerCode == "p1_" ? _p1AudioStart.PickRandom() : _p2AudioStart.PickRandom();
		_audioStreamPlayer.Stream = audio;
		_audioStreamPlayer.Play();
	}
	
	public void ChangeState(PlayerStates state)
	{
		if (_stateMachine.GetCurrentState() == PickleState) return;
		switch (state)
		{
			case PlayerStates.Default:
				_stateMachine.ChangeState(DefaultState);
				break;
			case PlayerStates.Microphone:
				_stateMachine.ChangeState(MicrophoneState);
				break;
			case PlayerStates.UnderControl:
				_stateMachine.ChangeState(UnderControlState);
				break;
			case PlayerStates.Chair:
				_stateMachine.ChangeState(SitDownState);
				break;
			case PlayerStates.Pickle:
				_stateMachine.ChangeState(PickleState);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(state), state, null);
		}
	}
	
	public PlayerStates GetState()
	{
		if (_stateMachine.GetCurrentState() == DefaultState)
			return PlayerStates.Default;
		else if (_stateMachine.GetCurrentState() == MicrophoneState)
			return PlayerStates.Microphone;
		else if (_stateMachine.GetCurrentState() == UnderControlState)
			return PlayerStates.UnderControl;
		return PlayerStates.Default;
	}

	#region States
	// Default State
	private void EnterDefaultState()
	{
		_hitBox.EnableArea();
		_microphoneSensor.EnableArea();
	}
	private void DefaultState()
	{
		if (IsOnFloor())
		{
			AnimSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
			if (Input.IsActionJustPressed(PlayerCode + "jump"))
			{
				Velocity = new Vector2(Velocity.X, _jumpStrength);
			}
			else if (Input.IsActionPressed(PlayerCode + "crouch"))
			{
				_stateMachine.ChangeState(CrouchedState);
			}
			else if (Input.IsActionJustPressed(PlayerCode + "special_a"))
			{
				if (CurrentPowerup is null) return;
				if (GetTree().GetFirstNodeInGroup(CurrentPowerup.Group) is not PowerupFactory powerupFactory) return;
				
				powerupFactory.ActivatePowerup(PlayerCode);
				CurrentPowerup = null;
			}
		}
		else
			_stateMachine.ChangeState(JumpState);
		MoveAndSlide();
	}
	
	// Jump State
	private void EnterJumpState()
	{
		_microphoneSensor.DisableArea();
		_canJump = true;
	}
	private void JumpState()
	{
		if (IsOnFloor())
		{
			_canJump = false;
			_stateMachine.ChangeState(DefaultState);
		}

		if (Input.IsActionJustPressed(PlayerCode + "jump") && _canJump)
		{
			Velocity = new Vector2(Velocity.X, _jumpStrength * 1.5f);
			_canJump = false;
		}
		
		AnimSprite.Play(Velocity.Y > 0.0f ? "fall" : "jump");

		MoveAndSlide();
	}
	private void LeaveJumpState()
	{
		_canJump = false;
		_microphoneSensor.EnableArea();
	}
	
	// MicrophoneState
	private void MicrophoneState()
	{
		CurrentPowerup = null;
		AnimSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		if (IsOnFloor())
		{
			AnimSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
			if (Input.IsActionJustPressed(PlayerCode + "jump"))
			{
				Velocity = new Vector2(Velocity.X, _jumpStrength);
			}
			else if (Input.IsActionJustPressed(PlayerCode + "crouch"))
			{
				_stateMachine.ChangeState(CrouchedState);
			}
		}
		else
			_stateMachine.ChangeState(JumpState);
		MoveAndSlide();
	}
	
	// UnderControll
	private void EnterUnderControlState()
	{
		AnimPlayer.Play("blink");
		_hitBox.DisableArea();
		_tomatoKiller.EnableArea();
		_microphoneSensor.DisableArea();
		CurrentPowerup = null;
		
		_controlTimer.Start();
	}
	private void UnderControlState()
	{
		AnimSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		MoveAndSlide();
	}
	private void LeaveUnderControlState()
	{
		AnimPlayer.Play("default");
		_hitBox.EnableArea();	
		_tomatoKiller.DisableArea();
		_microphoneSensor.EnableArea();
	}
	
	// Crouched State
	private void EnterCrouchedState()
	{
		AnimSprite.Play("crouch");
		_hitBox.DisableArea();
		_microphoneSensor.DisableArea();
	}
	private void CrouchedState()
	{
		AnimSprite.Play("crouch");
		if (Input.IsActionJustReleased(PlayerCode + "crouch"))
			_stateMachine.ChangeState(DefaultState);
	}
	private void LeaveCrouchedState()
	{
		_hitBox.EnableArea();
		_microphoneSensor.EnableArea();
	}
	
	// SitDown State
	private void SitDownState()
	{
		_hitBox.DisableArea();
		_microphoneSensor.DisableArea();
		AnimSprite.Play("chair");
	}
	
	// Pickle State
	private void EnterPickleState()
	{
		Pickle = true;
		_hitBox.DisableArea();
		_microphoneSensor.DisableArea();
		AnimPlayer.Play("pickle");
	}
	private void PickleState()
	{
		
	}
	#endregion
}