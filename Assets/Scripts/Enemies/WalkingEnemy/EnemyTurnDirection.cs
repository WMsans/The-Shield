using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyTurnDirection : EnemyAction
{
    bool _facingRight;
    public override void OnAwake()
    {
        _facingRight = FacingRight;
    }
    public override TaskStatus OnUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, _facingRight ? 0f : 180f);
        _facingRight = !_facingRight;
        return TaskStatus.Success;
    }
}
