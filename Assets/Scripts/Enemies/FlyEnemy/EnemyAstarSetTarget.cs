using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIDestinationSetter))]
public class EnemyAstarSetTarget : EnemyAction
{
    [SerializeField] private Transform target;
    AIDestinationSetter _destinationSetter;
    public override void OnStart()
    {
        base.OnStart();
        _destinationSetter = GetComponent<AIDestinationSetter>();
    }

    public override TaskStatus OnUpdate()
    {
        _destinationSetter.target = target;
        return TaskStatus.Success;
    }
}
