using System;
using Godot;
using Godot.Collections;
using GodotUtilities.Extensions;
using GodotUtilities.Logic;

namespace PunchLine.Entities;

public partial class PlayerCharacter : CharacterBody2D
{
	public enum PlayerStates
	{
		Default,
		Microphone,
		UnderControl,
		Chair
	}
	
	[Export] private Area2D _hitBox;
	[Export] private AnimatedSprite2D _animSprite;
	[Export] private AnimationPlayer _animPlayer;
	[Export] private Area2D _microphoneSensor;
	[Export] private Timer _controlTimer;
	[Export] private float _moveSpeed;
	[Export] private float _jumpStrength;
	[Export] private Area2D _tomatoKiller;
	[Export] private SpriteFrames _player1Sprite;
	[Export] private SpriteFrames _player2Sprite;
	
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public string PlayerCode { get; private set; }
	private string[] _playerCharacters = new[] { "casado", "hilario" };

	private RandomNumberGenerator _randomNumberGenerator;

	#region Flags

	private bool _canJump;

	#endregion

	private DelegateStateMachine _stateMachine;
	
	
	public override void _Ready()
	{
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
		
		_stateMachine.SetInitialState(SitDownState);
		
		PlayerCode = IsInGroup("Player1") ? "p1_" : "p2_";
		_animSprite.SpriteFrames = PlayerCode == "p1_" ? _player1Sprite : _player2Sprite;
		_animSprite.FlipH = IsInGroup("Player2");
	}
	
	public override void _PhysicsProcess(double delta)
	{
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
		
		_animSprite.FlipH = Velocity.X switch
		{
			< 0.0f => true,
			> 0.0f => false,
			_ => _animSprite.FlipH
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

	public void ChangeState(PlayerStates state)
	{
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
			_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
			if (Input.IsActionJustPressed(PlayerCode + "jump"))
			{
				Velocity = new Vector2(Velocity.X, _jumpStrength);
			}
			else if (Input.IsActionPressed(PlayerCode + "crouch"))
			{
				_stateMachine.ChangeState(CrouchedState);
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
		
		_animSprite.Play(Velocity.Y > 0.0f ? "fall" : "jump");

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
		_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		if (IsOnFloor())
		{
			_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
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
		_animPlayer.Play("blink");
		_hitBox.DisableArea();
		_tomatoKiller.EnableArea();
		_microphoneSensor.DisableArea();
		
		_controlTimer.Start();
	}
	private void UnderControlState()
	{
		_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		MoveAndSlide();
	}
	private void LeaveUnderControlState()
	{
		_animPlayer.Play("default");
		_hitBox.EnableArea();	
		_tomatoKiller.DisableArea();
		_microphoneSensor.EnableArea();
	}
	
	// Crouched State
	private void EnterCrouchedState()
	{
		_animSprite.Play("crouch");
		_hitBox.DisableArea();
		_microphoneSensor.DisableArea();
	}
	private void CrouchedState()
	{
		_animSprite.Play("crouch");
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
		_animSprite.Play("chair");
	}
	#endregion
}