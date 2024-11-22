using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyDetectLayer : EnemyConditional
{
    [SerializeField] private Transform wallDetectTransform;
    [SerializeField] private float wallDetectRadius;
    [SerializeField] private LayerMask wallMask;
    
    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(wallDetectTransform.position, wallDetectRadius);
    }
    public override TaskStatus OnUpdate()
    {
        return CheckWall() ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool CheckWall()
    {
        return Physics2D.OverlapCircle(wallDetectTransform.position, wallDetectRadius, wallMask);
    }
}
