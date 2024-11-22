using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Harmable : ShieldAttractingObject, IPersistant
{
    [SerializeField] float maxHitPoints = 10f;
    [SerializeField] private string id;
    [SerializeField] private bool persistant;
    [SerializeField] private float attractDistance;
    [SerializeField] private bool destroyOnDeath;
    public UnityEvent onDeath;
    public UnityEvent<Vector3> onHarm;
    public UnityEvent<float> onHeal;
    public float HitPoints { get; private set; }
    public bool Invincible { get; private set; }
    public bool IsAlive => HitPoints > 0;
    public float HitPointsNormalized => HitPoints / maxHitPoints;
    private Rigidbody2D _rb;
    public override float AttractDistance => attractDistance;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
    }
    public void Harm() => Harm(1f);
    public void Harm(float damage) => Harm(damage, Vector2.zero);
    public void Harm(float damage, Vector2 knockback){  
        if(Invincible) return;
        onHarm?.Invoke(new(damage, knockback.x, knockback.y));
        HitPoints -= damage;
        if(_rb)
        {
            _rb.velocity = knockback;
        }
        if (HitPoints <= 0)
        {
            Die();
        }
    }
    public void Heal() => Heal(1f);
    public void Heal(float amount)
    {
        onHeal?.Invoke(amount);
        HitPoints += amount;
    }

    private void Die()
    {
        SaveData();
        onDeath?.Invoke();
        if(destroyOnDeath)
            Destroy(gameObject);
    }

    public void SetInvincible(bool value, float duration)
    {
        Invincible = value;
        StartCoroutine(InvincibleCoroutine(duration));
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Invincible = !Invincible;
    }
    public override void OnReset()
    {
        base.OnReset();
        LoadData();
    }

    string IPersistant.Id
    {
        get => id;
        set => id = value;
    }

    public void SaveData()
    {
        if(!persistant) return;
        ES3.Save(id + "_HitPoints", HitPoints);
        ES3.Save(id + "_IsAlive", IsAlive);
    }

    void OnDisable()
    {
        SaveData();
    }
    public void LoadData()
    {
        if (!ES3.Load(id + "_IsAlive", true))
        {
            Destroy(gameObject);
            return;
        }
        HitPoints = ES3.Load(id + "_HitPoints", maxHitPoints);
    }

    [ContextMenu("Generate Guid")]
    public void GenerateGuid()
    {
        IPersistant.GenerateGuids(ref id);
    }
}
