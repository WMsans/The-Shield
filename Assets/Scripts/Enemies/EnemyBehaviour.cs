using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehaviour : MonoBehaviour
{
    protected abstract EnemyBaseState CurrentState { get; }
    protected abstract EnemyStats Stats { get; }
    protected abstract class EnemyBaseState
    {
        public abstract void OnEnterState(EnemyBehaviour enemy);
        public abstract void OnUpdate(EnemyBehaviour enemy);
        public abstract void OnFixedUpdate(EnemyBehaviour enemy);
        public abstract void OnExitState(EnemyBehaviour enemy);
    }

    protected virtual void Start()
    {
        SwitchState(Stats.startingState);
    }
    protected virtual void Update()
    {
        CurrentState.OnUpdate(this);
    }

    protected virtual void FixedUpdate()
    {
        CurrentState.OnFixedUpdate(this);
    }

    public abstract void SwitchState(Enums.EnemyStates newState);
}
