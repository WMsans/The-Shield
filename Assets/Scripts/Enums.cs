using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    #region State Machines
    public enum PlayerState
    {
        Normal,
        Defence,
    }

    public enum ShieldState
    {
        Hold, 
        Flying,
        Melee,
        Defence,
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
