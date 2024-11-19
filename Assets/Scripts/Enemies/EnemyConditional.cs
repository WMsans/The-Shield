
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyConditional : Conditional
{
    protected Rigidbody2D rb;
    protected Animator animator;

    public override void OnAwake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }
}
