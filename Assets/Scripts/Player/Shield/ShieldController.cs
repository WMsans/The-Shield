using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ShieldController : MonoBehaviour
{
    public static ShieldController Instance {get; private set;}
    public Rigidbody2D Rb { get; private set; }
    public bool DisCoolDown {get; set; }
    public ShieldStats stats;
    public HarmableStats harmableStats;
    public Enums.ShieldState CurrentState { get; private set; }

    private readonly Dictionary<Enums.ShieldState, ShieldBaseState> _states = new()
    {
        {Enums.ShieldState.Hold, new ShieldHoldState() },
        {Enums.ShieldState.Flying, new ShieldFlyingState() },
        {Enums.ShieldState.Melee, new ShieldMeleeState() },
        {Enums.ShieldState.Returning, new ShieldReturnState() },
        {Enums.ShieldState.Defense, new ShieldDefenseState() }, 
    };
    public float FireDownTimer { get; set; }
    public float DefenseDownTimer { get; set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one Shield in the scene");
            Destroy(gameObject);
        }
        Rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        TimerUpdate();
        GatherInput();
        _states[CurrentState].UpdateState(this);
    }

    void GatherInput()
    {
        if (Input.GetButtonDown("Fire1")) FireDownTimer = stats.PreInputTime;
        if (Input.GetButtonDown("Defense")) DefenseDownTimer = stats.PreInputTime;
    }

    void TimerUpdate()
    {
        FireDownTimer = Mathf.Max(0f, FireDownTimer - Time.deltaTime);
        DefenseDownTimer = Mathf.Max(0f, DefenseDownTimer - Time.deltaTime);
    }
    private void Start()
    {
        SwitchState(Enums.ShieldState.Hold);
    }

    void FixedUpdate()
    {
        _states[CurrentState].FixedUpdateState(this);
    }

    void LateUpdate()
    {
        _states[CurrentState].LateUpdateState(this);
    }
    public void SwitchState(Enums.ShieldState state)
    {
        if (_states.ContainsKey(CurrentState))
        {
            if (_states[CurrentState] != null)
                _states[CurrentState].ExitState(this);
        }
        CurrentState = state;
        _states[CurrentState].EnterState(this);
    }
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, stats.DetectionRadius);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stats == null) Debug.LogWarning("Please assign a ShieldStats asset to the Shield Controller's Stats slot", this);
    }
#endif
}
