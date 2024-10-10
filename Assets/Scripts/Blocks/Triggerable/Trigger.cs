using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trigger : ShieldAttractingObject
{
    protected abstract ITriggerable TriggeredObject { get; }
    public void OnTrigger()
    {
        TriggeredObject.OnTrigger();
    }

    public void OnUnTrigger()
    {
        TriggeredObject.OnUnTrigger();
    }
}
