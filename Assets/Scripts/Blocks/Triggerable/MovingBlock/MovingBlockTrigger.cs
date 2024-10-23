using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlockTrigger : Trigger
{
    [SerializeField] MovingBlock movingBlock;
    [SerializeField] bool allowUnTrigger;
    [SerializeField] float attractDistance;
    protected override ITriggerable TriggeredObject => movingBlock;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield"))
        {
            OnTrigger();
        }
    }

    private new void Awake()
    {
        base.Awake();
        AttractDistance = attractDistance;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnTrigger();
        }
    }
}
