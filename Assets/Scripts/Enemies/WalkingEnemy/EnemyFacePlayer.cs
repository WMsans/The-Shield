using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class EnemyFacePlayer : EnemyAction
{
    PlayerController _player;

    public override void OnStart()
    {
        _player = PlayerController.Instance;
    }

    public override TaskStatus OnUpdate()
    {
        if(_player == null) return TaskStatus.Failure;
        transform.eulerAngles = new(transform.eulerAngles.x,
            _player.transform.position.x > transform.position.x ? 0f : 180f, transform.eulerAngles.z);
        return TaskStatus.Success;
    }
}
