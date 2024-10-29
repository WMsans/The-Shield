using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarmable
{
    public float HitPoints { get; protected set; }
    public void Harm() => Harm(1f);
    public void Harm(float damage) => Harm(damage, new());
    public void Harm(float damage, Vector2 knockback){  
        HitPoints -= damage;
        if (HitPoints <= 0)
        {
            Die();
        }
    }
    public void Heal() => Heal(1f);
    public void Heal(float amount) => HitPoints += amount;
    public void Die(){}
}
