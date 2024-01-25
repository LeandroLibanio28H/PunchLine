using System;
using Godot;
using GodotUtilities.Logic;

namespace PunchLine.Entities;

public partial class PlayerCharacter : CharacterBody2D
{
	public enum PlayerStates
	{
		Default,
		Microphone,
		UnderControl
	}
	
	[Export] private CollisionShape2D _topHitbox;
	[Export] private CollisionShape2D _bottomHitbox;
	[Export] private AnimatedSprite2D _animSprite;
	[Export] private AnimationPlayer _animPlayer;
	[Export] private Area2D _microphoneSensor;
	[Export] private Timer _controlTimer;
	[Export] private float _moveSpeed;
	[Export] private float _jumpStrength;
	
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
		_stateMachine.AddStates(DefaultState);
		_stateMachine.AddStates(MicrophoneState);
		_stateMachine.AddStates(UnderControlState, EnterUnderControlState, LeaveUnderControlState);
		
		_stateMachine.SetInitialState(DefaultState);
		
		PlayerCode = IsInGroup("Player1") ? "p1_" : "p2_";
		_animSprite.SpriteFrames =
			GD.Load(
					$"res://scenes/entities/player/animations/{_playerCharacters[_randomNumberGenerator.RandiRange(0, _playerCharacters.Length - 1)]}.tres")
				as SpriteFrames;
		_animSprite.FlipH = IsInGroup("Player2");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity.Y += _gravity * (float)delta;
		}
		else
			_canJump = false;
		if (Input.IsActionJustPressed(PlayerCode + "jump"))
		{
			if (IsOnFloor() && !_canJump) 
			{
				velocity.Y = _jumpStrength;
				_canJump = true;
			}
			else if (_canJump)
			{
				velocity.Y = _jumpStrength * 1.5f;
				_canJump = false;
			}
		}
		var direction = Input.GetActionStrength(PlayerCode + "move_right") - 
		                Input.GetActionStrength(PlayerCode + "move_left");
		if (direction != 0.0f)
			velocity.X = direction * _moveSpeed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _moveSpeed);

		Velocity = velocity;
		_stateMachine.Update();
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
			default:
				throw new ArgumentOutOfRangeException(nameof(state), state, null);
		}
	}

	#region States
	// Default State
	private void DefaultState()
	{
		if (IsOnFloor())
		{
			_animSprite.FlipH = Velocity.X switch
			{
				< 0.0f => true,
				> 0.0f => false,
				_ => _animSprite.FlipH
			};
			_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		}
		MoveAndSlide();
	}
	
	// MicrophoneState
	private void MicrophoneState()
	{
		MoveAndSlide();
	}
	
	// UnderControll
	private void EnterUnderControlState()
	{
		_animPlayer.Play("blink");
		_topHitbox.Disabled = true;
		_bottomHitbox.Disabled = true;
		_microphoneSensor.Monitorable = false;
		_microphoneSensor.Monitoring = false;
		
		_controlTimer.Start();
	}
	private void UnderControlState()
	{
		if (IsOnFloor())
		{
			_animSprite.FlipH = Velocity.X switch
			{
				< 0.0f => true,
				> 0.0f => false,
				_ => _animSprite.FlipH
			};
			_animSprite.Play(Velocity.Length() > 0.0f ? "walk" : "idle");
		}
		MoveAndSlide();
	}

	private void LeaveUnderControlState()
	{
		_animPlayer.Play("default");
		_topHitbox.Disabled = false;
		_bottomHitbox.Disabled = false;	
		
		_microphoneSensor.Monitorable = true;
		_microphoneSensor.Monitoring = true;
	}
	#endregion
}