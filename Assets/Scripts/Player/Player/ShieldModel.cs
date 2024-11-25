using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ShieldModel : MonoBehaviour, ISaveable
{
    [SerializeField] private string id;
    [SerializeField] private float recoveryRate;
    private float _hp;
    private float _maxHp;
    public float ShieldHp { 
        get => _hp;
        set
        {
            _hp = value; 
            onHpChange?.Invoke(_hp);
        }
    }
    public bool OnDefence { get; set; }
    public bool IsDead { get; set; }
    public UnityEvent<float> onHpChange;
    public UnityEvent onDeath;
    public UnityEvent onReset;

    public void TakeDamage(float damage)
    {
        ShieldHp -= damage;
    }

    void Start()
    {
        LoadData();
    }

    void Update()
    {
        if (!OnDefence)
        {
            ShieldHp = Mathf.Clamp(_hp += recoveryRate * Time.deltaTime, 0, _maxHp);
            if(Mathf.Approximately(ShieldHp, _maxHp))
            {
                IsDead = false;
                onReset?.Invoke();
            }
        }
        else if (ShieldHp < 0f)
        {
            IsDead = true;
            onDeath?.Invoke();
            OnDefence = false;
        }
    }

    public void SaveData()
    {
        ES3.Save(id + "maxHp", _maxHp);
    }

    public void LoadData()
    {
        _maxHp = ES3.Load(id + "maxHp", 10);
    }
    [ContextMenu("Generate GUID")]
    public void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }
}
