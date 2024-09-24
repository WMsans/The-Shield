using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    #region State Machines
    public enum PlayerState
    {
        Normal,
    }

    public enum ShieldState
    {
        Hold, 
        Flying, 
        Returning,
    }
    #endregion
    
    #region Skills

    public enum Skill
    {
        None,
        Fixation,
    }
    #endregion
}
