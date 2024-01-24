using Godot;

namespace GodotUtilities.Logic;

using System.Collections.Generic;

public class DelegateStateMachine : RefCounted
{
    public delegate void State();

    private State? _currentState;

    private readonly Dictionary<State, StateFlows> _states = new();

    public void AddStates(State normal, State? enterState = null, State? leaveState = null)
    {
        var stateFlows = new StateFlows(normal, enterState, leaveState);
        _states[normal] = stateFlows;
    }

    public void ChangeState(State toStateDelegate)
    {
        _states.TryGetValue(toStateDelegate, out var stateDelegates);
        if (stateDelegates is null)
            return;
        Callable.From(() => SetState(stateDelegates)).CallDeferred();
    }

    public void SetInitialState(State stateDelegate)
    {
        _states.TryGetValue(stateDelegate, out var stateFlows);
        if (stateFlows is null)
            return;
        SetState(stateFlows);
    }

    public State? GetCurrentState() => _currentState;

    public void Update() => _currentState?.Invoke();

    private void SetState(StateFlows stateFlows)
    {
        if (_currentState != null)
        {
            _states.TryGetValue(_currentState, out var currentStateDelegates);
            currentStateDelegates?.LeaveState?.Invoke();
        }
        _currentState = stateFlows.Normal;
        stateFlows.EnterState?.Invoke();
    }

    private class StateFlows
    {
        public State Normal { get; private set; }
        public State? EnterState { get; private set; }
        public State? LeaveState { get; private set; }

        public StateFlows(State normal, State? enterState = null, State? leaveState = null)
        {
            Normal = normal;
            EnterState = enterState;
            LeaveState = leaveState;
        }
    }
}