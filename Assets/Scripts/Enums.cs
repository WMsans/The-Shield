using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    #region State Machines
    public enum PlayerState
    {
        Normal,
        Defense,
        Ledge,
    }

    public enum ShieldState
    {
        Hold, 
        Flying,
        Melee,
        Defense,
        Returning,
        Ledge,
        LedgeDefense,
    }
    #endregion
    
    #region Player Abilities
    public enum Skill
    {
        None,
        Fixation,
    }
    #endregion
}
