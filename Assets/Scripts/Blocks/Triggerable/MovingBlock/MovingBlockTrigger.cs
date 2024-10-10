using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlockTrigger : Trigger
{
    [SerializeField] MovingBlock movingBlock;
    [SerializeField] bool allowUnTrigger;
    protected override ITriggerable TriggeredObject => movingBlock;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield"))
        {
            OnTrigger();
        }
    }
}
