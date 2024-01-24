using Godot;
using GodotUtilities.Logic;

namespace PunchLine.Entities;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export] private CollisionShape2D _topHitbox;
	[Export] private CollisionShape2D _bottomHitbox;
	[Export] private float _moveSpeed;
	[Export] private float _jumpStrength;
	
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private string _playerCode;

	#region Flags

	private bool _canJump = false;

	#endregion

	private DelegateStateMachine _stateMachine;
	
	
	public override void _Ready()
	{
		_stateMachine = new DelegateStateMachine();
		_stateMachine.AddStates(DefaultState);
		_stateMachine.AddStates(MicrophoneState);
		_stateMachine.AddStates(UnderControllState);
		
		_stateMachine.SetInitialState(DefaultState);
		
		_playerCode = IsInGroup("Player1") ? "p1_" : "p2_";
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
		if (Input.IsActionJustPressed(_playerCode + "jump"))
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
		var direction = Input.GetActionStrength(_playerCode + "move_right") - 
		                Input.GetActionStrength(_playerCode + "move_left");
		if (direction != 0.0f)
			velocity.X = direction * _moveSpeed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _moveSpeed);

		Velocity = velocity;
		_stateMachine.Update();
	}


	#region States
	// Default State
	private void DefaultState()
	{
		MoveAndSlide();
	}
	
	// MicrophoneState
	private void MicrophoneState()
	{
		MoveAndSlide();
	}
	
	// UnderControll
	private void UnderControllState()
	{
		
	}
	#endregion
}