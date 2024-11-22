using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harmable : ShieldAttractingObject
{
    [SerializeField] float maxHitPoints = 10f;
    public float HitPoints { get; private set; }
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

    private void Die()
    {
        Destroy(gameObject);
    }

    public override void OnReset()
    {
        base.OnReset();
        HitPoints = maxHitPoints;
    }
}
