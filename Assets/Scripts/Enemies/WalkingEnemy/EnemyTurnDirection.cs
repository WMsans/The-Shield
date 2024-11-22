using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyTurnDirection : EnemyAction
{
    public override TaskStatus OnUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, FacingRight ? 180f : 0f, transform.eulerAngles.z);
        return TaskStatus.Success;
    }
}
