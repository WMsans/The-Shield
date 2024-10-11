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
        LedgeDefense,
    }

    public enum MovingBlockState
    {
        Idle,
        Dashing,
        Returning,
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
