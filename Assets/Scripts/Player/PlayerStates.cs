    using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class PlayerBaseState
{
    public abstract void EnterState(PlayerController player);
    public abstract void UpdateState(PlayerController player);
    public abstract void FixedUpdateState(PlayerController player);
    public abstract void ExitState(PlayerController player);
}

public class PlayerNormalState : PlayerBaseState, IPlayerController
{
    private PlayerStats stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    
    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;

    public override void EnterState(PlayerController player)
    {
        stats = player.stats;
        
        _rb = player.Rd;
        _col = player.GetComponent<CapsuleCollider2D>();
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

        if (stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
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
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, stats.GrounderDistance, stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, stats.GrounderDistance, stats.GroundLayer);

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

    }

    #endregion


    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + stats.CoyoteTime;

    private void HandleJump(PlayerController player)
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _rb.velocity = new(_rb.velocity.x, stats.JumpPower);
        Jumped?.Invoke();
    }

    private void CheckForBounced(PlayerController player)
    {
        if (player.Bounced)
        {
            if(_grounded) player.Bounced = false;
        }
    }
    #endregion

    #region Horizontal

    private void HandleDirection(PlayerController player)
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, 0, deceleration * Time.fixedDeltaTime), _rb.velocity.y);
        }
        else if (player.Bounced && Mathf.Abs(_rb.velocity.x) > stats.MaxBouncedSpeed && Mathf.Approximately(Mathf.Sign(_frameInput.Move.x), Mathf.Sign(_rb.velocity.x)))
        {
            var deceleration = _grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, _frameInput.Move.x * stats.MaxBouncedSpeed, deceleration * Time.fixedDeltaTime), _rb.velocity.y);
        }
        else if(!(_rb.velocity.x < stats.MaxBouncedSpeed && _rb.velocity.x > stats.MaxSpeed))
        {
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, _frameInput.Move.x * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime), _rb.velocity.y);
        }
    }
    
    #endregion

    #region Gravity

    private void HandleGravity(PlayerController player)
    {
        if (_grounded && _rb.velocity.y <= 0f)
        {
            _rb.velocity = new(_rb.velocity.x, stats.GroundingForce);
        }
        else
        {
            var inAirGravity = stats.FallAcceleration;
            if (_endedJumpEarly && _rb.velocity.y > 0 && !player.Bounced) inAirGravity *= stats.JumpEndEarlyGravityModifier;
            _rb.velocity = new(_rb.velocity.x, Mathf.MoveTowards(_rb.velocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime));
        }
    }

    #endregion


    public override void ExitState(PlayerController player)
    {
        
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