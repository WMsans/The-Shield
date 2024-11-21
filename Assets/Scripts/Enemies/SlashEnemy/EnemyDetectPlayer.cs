using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyDetectPlayer : EnemyConditional
{
    [SerializeField] private Collider2D vision;
    [SerializeField] private LayerMask playerLayer;
    
    public override TaskStatus OnUpdate()
    {
        return vision.IsTouchingLayers(playerLayer) ? TaskStatus.Success : TaskStatus.Failure;
    }
}
