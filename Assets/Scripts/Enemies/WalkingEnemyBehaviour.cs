using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WalkingEnemyBehaviour : EnemyBehaviour
{
    [SerializeField] EnemyStats stats;
    private WalkingBaseState _currentState;
    protected override EnemyBaseState CurrentState => _currentState;
    protected override EnemyStats Stats => stats;

    protected abstract class WalkingBaseState : EnemyBaseState
    {
        public static implicit operator WalkingBaseState(Enums.EnemyStates src)
        {
            return src switch
            {
                Enums.EnemyStates.Walking => new WalkingWalkState(),
                _ => null
            };
        }
    }

    private class WalkingWalkState : WalkingBaseState
    {
        public override void OnEnterState(EnemyBehaviour enemy)
        {
            throw new NotImplementedException();
        }

        public override void OnUpdate(EnemyBehaviour enemy)
        {
            throw new NotImplementedException();
        }

        public override void OnFixedUpdate(EnemyBehaviour enemy)
        {
            throw new NotImplementedException();
        }

        public override void OnExitState(EnemyBehaviour enemy)
        {
            throw new NotImplementedException();
        }
    }

    public override void SwitchState(Enums.EnemyStates newState)
    {
        _currentState?.OnExitState(this);
        _currentState = newState;
        _currentState?.OnEnterState(this);
    }
}
