using System;
using UnityEditor;
using UnityEngine;

public abstract class PlayerBaseState
{
    public abstract void EnterState(PlayerController player);
    public abstract void UpdateState(PlayerController player);
    public abstract void FixedUpdateState(PlayerController player);
    public abstract void ExitState(PlayerController player);
}

public class PlayerNormalState : PlayerBaseState, IPlayerController
{
    private PlayerStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Transform _ledgeCheck;
    private Transform _ledgeBodyCheck;
    private float _ledgeCheckRadius;
    
    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;

    public override void EnterState(PlayerController player)
    {
        _stats = player.stats;
        
        _rb = player.Rb;
        _col = player.GetComponent<CapsuleCollider2D>();
        _ledgeCheck = player.grabPoint;
        _ledgeBodyCheck = player.grabBodyPoint;
        _ledgeCheckRadius = player.grabRadius;
    }

    public override void UpdateState(PlayerController player)
    {
        _time += Time.deltaTime;
        
        GatherInput();
    }
    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") ,
            JumpHeld = Input.GetButton("Jump") ,
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
    }

    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollisions(player);
        CheckForBounced(player);

        HandleJump(player);
        HandleDirection(player);
        HandleGravity(player);
    }
    #region Collisions
    
    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions(PlayerController player)
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);

        // Hit a Ceiling
        if (ceilingHit)
        {
            _rb.velocity = new(_rb.velocity.x, Mathf.Min(0, _rb.velocity.y));
        }

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_rb.velocity.y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }
        // Check for ledge climbing
        var ledgeRay = Physics2D.Raycast(_ledgeCheck.position, Vector2.right * (player.FacingRight ? 1 : -1), _ledgeCheckRadius, _stats.GroundLayer);
        var bodyRay = Physics2D.Raycast(_ledgeBodyCheck.position, Vector2.right * (player.FacingRight ? 1 : -1), _ledgeCheckRadius, _stats.GroundLayer);
        if (!ledgeRay && bodyRay && !_grounded && _rb.velocity.y < 0)
        {
            // Ledge climbing! Find the ledge point
            var ray = Physics2D.Raycast((Vector2)_ledgeCheck.position + Vector2.right * ((player.FacingRight ? 1 : -1) * _ledgeCheckRadius), Vector2.down, Mathf.Infinity, _stats.GroundLayer);
            player.LedgePoint = new (bodyRay.point.x, ray.point.y);
            player.SwitchState(Enums.PlayerState.Ledge);
        }
    }

    #endregion


    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump(PlayerController player)
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote)
        {
            ExecuteJump();
            if (player.Bounced) player.SuperBounce();
        }

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _rb.velocity = new(_rb.velocity.x, _stats.JumpPower);
        Jumped?.Invoke();
    }

    private void CheckForBounced(PlayerController player)
    {
        if (player.Bounced)
        {
            if(_grounded && !player.ShieldPushed) player.Bounced = false;
        }
    }
    #endregion

    #region Horizontal

    private void HandleDirection(PlayerController player)
    {
        if ((!Mathf.Approximately(Mathf.Sign(_frameInput.Move.x), Mathf.Sign(_rb.velocity.x)) ||
             _frameInput.Move.x == 0) && player.Bounced)
        {
            player.Bounced = false;
        }
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, 0, 
                deceleration * Time.fixedDeltaTime), _rb.velocity.y);
        }
        else if (player.Bounced && Mathf.Abs(_rb.velocity.x) > _stats.MaxBouncedSpeed && 
                 Mathf.Approximately(Mathf.Sign(_frameInput.Move.x), Mathf.Sign(_rb.velocity.x))) 
        {
            // Speed higher than bounced max speed, direct constrain
            _rb.velocity = new(_frameInput.Move.x * _stats.MaxBouncedSpeed, _rb.velocity.y);
        }
        else if (!player.Bounced && Mathf.Abs(_rb.velocity.x) > _stats.MaxSpeed &&
                 Mathf.Approximately(Mathf.Sign(_frameInput.Move.x), Mathf.Sign(_rb.velocity.x)))
        {
            // Speed higher than max speed, direct constrain
            _rb.velocity = new(_frameInput.Move.x * _stats.MaxSpeed, _rb.velocity.y);
        }
        else if(!( Mathf.Abs(_rb.velocity.x) < _stats.MaxBouncedSpeed && Mathf.Abs(_rb.velocity.x) > _stats.MaxSpeed && player.Bounced))
        {
            // Normal
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, _frameInput.Move.x * _stats.MaxSpeed,
                (player.ShieldPushed ? _stats.PushAcceleration * _stats.Acceleration : _stats.Acceleration) * Time.fixedDeltaTime), _rb.velocity.y);
        }
        // Player rotation
        if (_frameInput.Move.x != 0 && player.FacingRight ^ (_rb.velocity.x > 0))
        {
            player.FacingRight = _rb.velocity.x > 0;
            player.transform.rotation = Quaternion.Euler(0f, player.FacingRight ? 0 : -180f, 0f);
        }
    }
    
    #endregion

    #region Vertical 

    private void HandleGravity(PlayerController player)
    {
        if (_grounded && _rb.velocity.y <= 0f)
        {
            _rb.velocity = new(_rb.velocity.x, _stats.GroundingForce);
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _rb.velocity.y > 0 && !player.Bounced && !player.ShieldPushed) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _rb.velocity = new(_rb.velocity.x, Mathf.MoveTowards(_rb.velocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime));
        }
    }

    #endregion
    public override void ExitState(PlayerController player)
    {
        
    }
}

public class PlayerDefenseState : PlayerBaseState
{
    private Rigidbody2D _rd;
    private PlayerStats _stats;
    private CapsuleCollider2D _col;
    private bool _grounded;
    private bool _jumpHeld;
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Player Defense!");
        _rd = player.Rb;
        _stats = player.stats;
        _col = player.GetComponent<CapsuleCollider2D>();
    }

    public override void UpdateState(PlayerController player)
    {
        GatherInput();
    }

    private void GatherInput()
    {
        _jumpHeld = Input.GetButton("Jump");
    }
    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollisions();
        HandleGravity(player);
        
        var decel = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
        _rd.velocity = new (Mathf.MoveTowards(_rd.velocity.x, 0, decel * Time.fixedDeltaTime), _rd.velocity.y);
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);

        // Hit a Ceiling
        if (ceilingHit)
        {
            _rd.velocity = new(_rd.velocity.x, Mathf.Min(0, _rd.velocity.y));
        }

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
        }
    }
    private void HandleGravity(PlayerController player)
    {
        if (_grounded && _rd.velocity.y <= 0f)
        {
            _rd.velocity = new(_rd.velocity.x, _stats.GroundingForce);
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_rd.velocity.y > 0 && !player.Bounced && !player.ShieldPushed) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _rd.velocity = new(_rd.velocity.x, Mathf.MoveTowards(_rd.velocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime));
        }
    }
    public override void ExitState(PlayerController player)
    {
        
    }
}

public class PlayerLedgeState : PlayerBaseState
{
    private ShieldController _shield;
    private Rigidbody2D _rb;

    private float _jumpTimer;
    PlayerStats _stats;
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Climb Edge");
        
        _shield = ShieldController.Instance;
        _shield.SwitchState(Enums.ShieldState.Ledge);
        _rb = player.Rb;
        _stats = player.stats;
        _jumpTimer = 0f;
    }

    public override void UpdateState(PlayerController player)
    {
        GatherInput();
    }

    void GatherInput()
    {
        _jumpTimer = Mathf.Max(0f, _jumpTimer - Time.deltaTime);
        if (Input.GetButtonDown("Jump"))
        {
            _jumpTimer = _stats.JumpBuffer;
        }
    }
    public override void FixedUpdateState(PlayerController player)
    {
        // Fixed to the ledge position
        _rb.velocity = Vector2.zero;
        _rb.position = new(_rb.position.x, player.LedgePoint.y + _rb.position.y - player.grabPoint.position.y);
        // Jump detection
        HandleJump(player);
    }

    void HandleJump(PlayerController player)
    {
        if(_jumpTimer > 0)
        {
            _rb.velocity = new(_rb.velocity.x, _stats.JumpPower);
            player.SwitchState(Enums.PlayerState.Normal);
        }
    }
    public override void ExitState(PlayerController player)
    {
        _shield.SwitchState(Enums.ShieldState.Hold);
    }
}
public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}