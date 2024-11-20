using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyDetectLayer : EnemyConditional
{
    [SerializeField] private Vector2 wallDetectPosition;
    [SerializeField] private float wallDetectRadius;
    [SerializeField] private LayerMask wallMask;
    
    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(wallDetectPosition, wallDetectRadius);
    }
    public override TaskStatus OnUpdate()
    {
        return CheckWall() ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool CheckWall()
    {
        return Physics2D.OverlapCircle(rb.position + wallDetectPosition * new Vector2(FacingRight ? 1 : -1, 1), wallDetectRadius, wallMask);
    }
}
