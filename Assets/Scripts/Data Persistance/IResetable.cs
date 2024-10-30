using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetable 
{
    bool Initialized { get; set; }
    public void OnInitialize();
    public void OnReset();
}
