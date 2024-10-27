using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetable 
{
    bool Initialized { get; protected set; }
    public void OnInitialize();
    public void OnReset();
}
