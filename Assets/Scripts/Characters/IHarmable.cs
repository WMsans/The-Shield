using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarmable
{
    public float HitPoints { get; protected set; }
    public void Harm(){ Harm(1f);}
    public void Harm(float damage) { HitPoints -= damage;}
    public void Harm(float damage, Vector2 knockback){ Harm(damage);}
    public void Heal() {Heal(1f);}
    public void Heal(float amount){HitPoints += amount;}
}
