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
    private PlayerStats _stats;
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
        _stats = player.stats;
        
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

public class PlayerDefenceState : PlayerBaseState
{
    public override void EnterState(PlayerController player)
    {
        
    }

    public override void UpdateState(PlayerController player)
    {
        
    }

    public override void FixedUpdateState(PlayerController player)
    {
        
    }

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