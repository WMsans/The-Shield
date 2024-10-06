using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    #region Movements
    private float _pressingHor;
    public PlayerStats stats;
    public bool Bounced { get; set; }
    public bool FacingRight { get; set; } = true;
    private float _bouncedTimer;
    public bool ShieldPushed { get; private set; }
    private float _shieldPushTimer;
    public Transform grabPoint;
    public Transform grabBodyPoint;
    public float grabRadius;
    public Vector2 LedgePoint { get; set; }
    public Rigidbody2D Rb { get; private set; }
    #endregion
    #region State manchine
    
    public Enums.PlayerState CurrentState { get; private set; }

    private readonly Dictionary<Enums.PlayerState, PlayerBaseState> _states = new()
    {
        {Enums.PlayerState.Normal, new PlayerNormalState() },
        {Enums.PlayerState.Defense, new PlayerDefenseState() }, 
        {Enums.PlayerState.Ledge, new PlayerLedgeState() }
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
        Rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SwitchState(Enums.PlayerState.Normal);

        _shieldPushTimer = 0f;
        FacingRight = true;
    }
    
    private void Update()
    {
        GatherInput();
        _states[CurrentState].UpdateState(this);
        
        _shieldPushTimer -= Time.deltaTime;
        if(_shieldPushTimer <= 0) ShieldPushed = false;
        if (!ShieldPushed) _shieldPushTimer = 0f;
        
        _bouncedTimer -= Time.deltaTime;
        if(_bouncedTimer <= 0) Bounced = false;
        if(!Bounced) _bouncedTimer = 0f;
    }

    void GatherInput()
    {
        _pressingHor = Input.GetAxisRaw("Horizontal");
    }
    private void FixedUpdate()
    {
        _states[CurrentState].FixedUpdateState(this);
    }

    /// <summary>
    /// Push the player against the direction and activate the bounce and push timer
    /// </summary>
    /// <param name="dir">Direction in normal Vector2</param>
    /// <param name="force">Force given to player</param>
    /// <returns>If the player did a neutral bounce</returns>
    public bool ShieldPush(Vector2 dir, float force)
    {
        Rb.velocity -= dir * force;
        Bounced = true;
        StartBounceTimer();
        ShieldPushed = true;
        _shieldPushTimer = stats.PushAccelerationSustainTime;
        
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (Mathf.DeltaAngle(rot, 270f) < 30f)
        {
            // If push upward, check if player is beside a wall
            var ray = Physics2D.Raycast(Rb.position, Vector2.right, stats.WallerDistance, stats.GroundLayer);
            if (ray.collider != null)
            {
                if(!Mathf.Approximately(_pressingHor, 0f) && Mathf.Sign(_pressingHor) >= 1f) // Push player against the wall
                {
                    Rb.velocity = new(-stats.WallingForce, Rb.velocity.y);
                    return false;
                }
                else // Neutral Jump
                {
                    Rb.velocity = new(-stats.WallingNeutralForce, Rb.velocity.y);
                    return true;
                }
            }
            ray = Physics2D.Raycast(Rb.position, Vector2.left, stats.WallerDistance, stats.GroundLayer);
            if (ray.collider != null)
            {
                if(!Mathf.Approximately(_pressingHor, 0f) && Mathf.Sign(_pressingHor) <= -1f) // Push player against the wall
                {
                    Rb.velocity = new(stats.WallingForce, Rb.velocity.y);
                    return false;
                }
                else // Neutral Jump
                {
                    Rb.velocity = new(stats.WallingNeutralForce, Rb.velocity.y);
                    return true;
                }
            }
        }
        return false;
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
        if (_states.ContainsKey(CurrentState))
        {
            if (_states[CurrentState] != null)
                _states[CurrentState].ExitState(this);
        }
        CurrentState = state;
        _states[CurrentState].EnterState(this);
    }
    
    public PlayerBaseState GetStateInstance(Enums.PlayerState state)
    {
        return _states[state];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(grabPoint.position, grabPoint.position + grabRadius * Vector3.right);
        Gizmos.DrawLine(grabBodyPoint.position, grabBodyPoint.position + grabRadius * Vector3.right);
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stats == null) Debug.LogWarning("Please assign a PlayerStats asset to the Player Controller's Stats slot", this);
        if(grabPoint == null) Debug.LogWarning("Please assign a grab point transform to the Player Controller's Grab Point", this);
        if(grabBodyPoint == null) Debug.LogWarning("Please assign a grab body point transform to the Player Controller's Grab grab Point", this);
    }
#endif
}



