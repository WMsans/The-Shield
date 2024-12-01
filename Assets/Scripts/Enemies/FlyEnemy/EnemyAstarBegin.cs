using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class EnemyAstarBegin : EnemyAction
{
    AIPath _path;
    public override void OnStart()
    {
        base.OnStart();
        _path = GetComponent<AIPath>();
    }

    public override TaskStatus OnUpdate()
    {
        _path.canMove = true;
        return TaskStatus.Success;
    }
}