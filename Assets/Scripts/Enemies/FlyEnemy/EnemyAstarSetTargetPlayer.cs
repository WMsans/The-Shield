using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIDestinationSetter))]
public class EnemyAstarSetTargetPlayer : EnemyAction
{
    AIDestinationSetter destinationSetter;
    Transform target;

    public override void OnStart()
    {
        base.OnStart();
        destinationSetter = GetComponent<AIDestinationSetter>();
        target = PlayerController.Instance.transform;
    }

    public override TaskStatus OnUpdate()
    {
        destinationSetter.target = target;
        return TaskStatus.Success;
    }
}
