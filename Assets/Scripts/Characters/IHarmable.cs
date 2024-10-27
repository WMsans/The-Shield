using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarmable
{
    public float HitPoints { get; protected set; }
    public virtual void Harm(){ Harm(1f);}
    public virtual void Harm(float damage) {Harm(damage, new()); }
    public virtual void Harm(float damage, Vector2 knockback){  HitPoints -= damage;}
    public virtual void Heal() {Heal(1f);}
    public virtual void Heal(float amount){HitPoints += amount;}
}
