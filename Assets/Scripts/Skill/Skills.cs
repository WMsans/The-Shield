using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill
{
    public abstract string skillName { get; }
    public abstract string skillDescription { get; }

    public abstract void StartSkill();
    public abstract void EndSkill();
    public abstract void UpdateSkill();
    public abstract void FixedUpdateSkill();
}

public class ShieldFixationSkill : BaseSkill
{
    public override string skillName { get; } = "Shield Fixation";
    public override string skillDescription { get; } = "Hold shield at a position for a amount of time."; 

    public override void StartSkill()
    {
        throw new System.NotImplementedException();
    }

    public override void EndSkill()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateSkill()
    {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateSkill()
    {
        throw new System.NotImplementedException();
    }
}