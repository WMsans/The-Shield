using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyTurnDirection : EnemyAction
{
    public override TaskStatus OnUpdate()
    {
        transform.Rotate(new Vector3(0f, 0f, 180f));
        return TaskStatus.Success;
    }
}
