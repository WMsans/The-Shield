using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    #region Movements
    public PlayerStats stats;
    public bool Bounced { get; set; }
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
    }
    
    private void Update()
    {
        states[CurrentState].UpdateState(this);
    }
    
    private void FixedUpdate()
    {
        states[CurrentState].FixedUpdateState(this);
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



