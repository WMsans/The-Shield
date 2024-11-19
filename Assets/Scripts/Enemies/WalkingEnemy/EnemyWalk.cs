using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyWalk : EnemyAction
{
    public float WalkSpeed = 1.5f;
    
    public override TaskStatus OnUpdate()
    {
        rb.velocity = Mathf.Cos(transform.rotation.z * Mathf.Deg2Rad) * WalkSpeed * Vector2.right;
        return TaskStatus.Running;
    }
}
