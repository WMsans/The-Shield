using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    public Enums.Skill CurrentSkill { get; private set;}
    Dictionary<Enums.Skill, BaseSkill> skills = new()
    {
        {Enums.Skill.Fixation, new ShieldFixationSkill() }
    };
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one SkillManager in the scene!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CurrentSkill = Enums.Skill.None;
    }
    
}
