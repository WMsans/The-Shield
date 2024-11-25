using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetable 
{
    public void OnInitialize();
    public void OnReset();
}
