using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetable 
{
    bool Initialized { get; set; }
    public abstract void OnInitialize();
    public abstract void OnReset();
}
