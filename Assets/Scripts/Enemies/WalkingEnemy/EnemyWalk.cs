using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyWalk : EnemyAction
{
    public float WalkSpeed = 1.5f;
    
    public override TaskStatus OnUpdate()
    {
        rb.velocity = new(WalkSpeed * (FacingRight ? 1 : -1), rb.velocity.y);
        return TaskStatus.Running;
    }
}
