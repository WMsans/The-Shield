using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill
{
    public abstract string skillName { get; }
    public abstract string skillDescription { get; }

    public abstract void StartSkill(SkillManager manager);
    public abstract void EndSkill(SkillManager manager);
    public abstract void UpdateSkill(SkillManager manager);
    public abstract void FixedUpdateSkill(SkillManager manager);
}

public class ShieldFixationSkill : BaseSkill
{
    public override string skillName { get; } = "Shield Fixation";
    public override string skillDescription { get; } = "Hold shield at a position for a amount of time."; 

    public override void StartSkill(SkillManager manager)
    {
        throw new System.NotImplementedException();
    }

    public override void EndSkill(SkillManager manager)
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateSkill(SkillManager manager)
    {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateSkill(SkillManager manager)
    {
        throw new System.NotImplementedException();
    }
}