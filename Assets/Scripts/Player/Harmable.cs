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
    [SerializeField] private ShieldModel shieldModel;
    [SerializeField] private bool frozeTime = false;
    [SerializeField] private float frozenScale = 0.01f;
    [SerializeField] private float frozenTimeDuration = 0.05f;
    [SerializeField] private bool changeChromaticAberration;
    [SerializeField] private bool shakeCamera;
    public UnityEvent onDeath;
    public UnityEvent<Vector3> onHarm;
    public UnityEvent<float> onHeal;
    public UnityEvent<float> onShield;
    public float HitPoints { get; private set; }
    public bool Invincible { get; private set; }
    public bool Shielded { get; set; }
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
        if(frozeTime)
        {
            TimeManager.Instance?.FrozenTime(frozenTimeDuration, frozenScale);
        }

        if (changeChromaticAberration)
        {
            GlobalVolumeManager.Instance?.ChangeChromaticAberration(1f, 0f, 0.3f, BetterLerp.LerpType.Sin, true);
            RadialBlurManager.Instance?.UpdateMaterial(-0.1f, 0f, 0.3f, BetterLerp.LerpType.Sin, true);
        }
        
        if (shakeCamera)
        {
            CameraShake.Instance?.ShakeCamera(0.1f, 0.3f);
        }
    }

    public void OnShielded(float damage)
    {
        shieldModel?.TakeDamage(damage);
        onShield?.Invoke(damage);
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
            Activate(false);
    }
    private void Activate(bool value)
    {
        // Get all colliders and renderer of the object
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Disable all colliders and renderers
        foreach (Collider2D c in colliders)
        {
            c.enabled = value;
        }
        foreach (Renderer r in renderers)
        {
            r.enabled = value;
        }
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
        HitPoints = maxHitPoints;
        Activate(true);
        LoadData();
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
        if (!persistant) return;
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
