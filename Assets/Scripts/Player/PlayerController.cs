using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    #region Movements
    public PlayerStats stats;
    public bool Bounced { get; set; }
    private float _bouncedTimer;
    public bool ShieldPushed { get; private set; }
    private float _shieldPushTimer;
    public Rigidbody2D Rd { get; private set; }
    #endregion
    #region State manchine
    
    public Enums.PlayerState CurrentState { get; private set; }
    Dictionary<Enums.PlayerState, PlayerBaseState> states = new()
    {
        {Enums.PlayerState.Normal, new PlayerNormalState() }
    };
    
    #endregion
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one instance of PlayerController in the scene");
            Destroy(gameObject);
        }
        Rd = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SwitchState(Enums.PlayerState.Normal);

        _shieldPushTimer = 0f;
    }
    
    private void Update()
    {
        states[CurrentState].UpdateState(this);
        
        _shieldPushTimer -= Time.deltaTime;
        if(_shieldPushTimer <= 0) ShieldPushed = false;
        if (!ShieldPushed) _shieldPushTimer = 0f;
        
        _bouncedTimer -= Time.deltaTime;
        if(_bouncedTimer <= 0) Bounced = false;
        if(!Bounced) _bouncedTimer = 0f;
        
        print(_bouncedTimer + " " + Bounced);
    }
    
    private void FixedUpdate()
    {
        states[CurrentState].FixedUpdateState(this);
    }

    public void ShieldPush()
    {
        ShieldPushed = true;
        _shieldPushTimer = stats.PushAccelerationSustainTime;
    }

    public void StartBounceTimer()
    {
        _bouncedTimer = stats.BounceTime;
    }

    public void SuperBounce()
    {
        _bouncedTimer = Mathf.Infinity;
    }
    public void SwitchState(Enums.PlayerState state)
    {
        if (states.ContainsKey(CurrentState))
        {
            if (states[CurrentState] != null)
                states[CurrentState].ExitState(this);
        }
        CurrentState = state;
        states[CurrentState].EnterState(this);
    }
    
    public PlayerBaseState GetStateInstance(Enums.PlayerState state)
    {
        return states[state];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stats == null) Debug.LogWarning("Please assign a PlayerStats asset to the Player Controller's Stats slot", this);
    }
#endif
}



