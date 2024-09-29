using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ShieldController : MonoBehaviour
{
    public static ShieldController Instance {get; private set;}
    public Rigidbody2D Rd { get; private set; }
    public bool DisCoolDown {get; set; }
    public ShieldStats stats;
    public Enums.ShieldState CurrentState { get; private set; }
    Dictionary<Enums.ShieldState, ShieldBaseState> states = new()
    {
        {Enums.ShieldState.Hold, new ShieldHoldState() },
        {Enums.ShieldState.Flying, new ShieldFlyingState() },
        {Enums.ShieldState.Returning, new ShieldReturnState()}
    };
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
        Rd = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        states[CurrentState].UpdateState(this);
    }
    private void Start()
    {
        SwitchState(Enums.ShieldState.Hold);
    }

    void FixedUpdate()
    {
        states[CurrentState].FixedUpdateState(this);
    }

    void LateUpdate()
    {
        states[CurrentState].LateUpdateState(this);
    }
    public void SwitchState(Enums.ShieldState state)
    {
        if (states.ContainsKey(CurrentState))
        {
            if (states[CurrentState] != null)
                states[CurrentState].ExitState(this);
        }
        CurrentState = state;
        states[CurrentState].EnterState(this);
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
