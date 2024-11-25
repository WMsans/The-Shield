using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class PlayerBaseState
{
    public virtual void EnterState(PlayerController player) {}
    public virtual void UpdateState(PlayerController player) {}
    public virtual void FixedUpdateState(PlayerController player){}
    public virtual void ExitState(PlayerController player){}

    public virtual void HarmState(PlayerController player, float damage, Vector2 knockBack)
    {
        PlayerStatsManager.Instance.PlayerHealth -= damage;
        player.StartCoroutine(player.InvincibleTimer());
        // Flash
        player.damageFlash.Flash(player.stats.InvincibilityTime);
    }
}
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}
public class PlayerNormalState : PlayerBaseState
{
    private PlayerStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Transform _ledgeCheck;
    private Transform _ledgeBodyCheck;
    private float _ledgeCheckRadius;
    private float _timeGrabDownWasPressed;
    private Vector2 FrameInput => _frameInput.Move;
    Vector2 _preBouncedVelocity;

    private float _time;

    public override void EnterState(PlayerController player)
    {
        InitializeVariables(player);
    }

    void InitializeVariables(PlayerController player)
    {
        _stats = player.stats;
        
        _rb = player.Rb;
        _col = player.Col;
        _ledgeCheck = player.grabPoint;
        _ledgeBodyCheck = player.grabBodyPoint;
        _ledgeCheckRadius = player.grabRadius;

        _endedJumpEarly = false;
    }
    public override void UpdateState(PlayerController player)
    {
        _time += Time.deltaTime;
        
        GatherInput();
        if (!_frameInput.JumpDown && FrameInput.y < 0)
        {
            player.SwitchState(Enums.PlayerState.Crouch);
        }
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
        }else if (Input.GetButtonDown("Ledge"))
        {
            _timeGrabDownWasPressed = _time;
        }
    }

    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollisions(player);
        CheckGrabDownLedge(player);
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
        var groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        var ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);
        var crashCol = player.IsCrashed();
        
        if(crashCol)
        {
            player.ReturnToSpawn();
            return;
        }
        // Hit a Ceiling
        if (ceilingHit && !ceilingHit.collider.isTrigger && !ceilingHit.collider.CompareTag("OneWayPlatform"))
        {
            _rb.velocity = new(_rb.velocity.x, Mathf.Min(0, _rb.velocity.y));
        }

        // Landed on the Ground
        if (!_grounded && groundHit && !groundHit.collider.isTrigger)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
        }
        // Left the Ground
        else if (_grounded && !groundHit.collider)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
        }
        // Check for ledge climbing
        var ledgeRay = Physics2D.Raycast(_ledgeCheck.position, Vector2.right * (player.FacingRight ? 1 : -1), _ledgeCheckRadius, _stats.GroundLayer);
        var bodyRay = Physics2D.Raycast(_ledgeBodyCheck.position, Vector2.right * (player.FacingRight ? 1 : -1), _ledgeCheckRadius, _stats.GroundLayer);
        if (!ledgeRay && bodyRay && !_grounded && _rb.velocity.y < 0)
        {
            // Ledge climbing! Find the ledge point
            var ray = Physics2D.Raycast((Vector2)_ledgeCheck.position + Vector2.right * ((player.FacingRight ? 1 : -1) * _ledgeCheckRadius), Vector2.down, 1f, _stats.GroundLayer);
            if (ray.collider.CompareTag("MovingBlock"))
            {
                player.anchorPointBehaviour.SetTarget(ray.transform);
            }
            player.LedgePoint = new (bodyRay.point.x, ray.point.y);
            player.SwitchState(Enums.PlayerState.Ledge);
        }
    }
    private bool HasBufferedLedge => _time < _timeGrabDownWasPressed + _stats.LedgeBuffer;
    void CheckGrabDownLedge(PlayerController player)
    {
        var col = Physics2D.OverlapCircle(player.grabDownPoint.position, player.grabDownRadius, _stats.GroundLayer);
        if(col != null || !_grounded) return;
        // If no wall in front of the player, check for ledging
        if (HasBufferedLedge)
        {
            // Find the ledge point
            var ray = Physics2D.Raycast(player.grabDownPoint.position, Vector2.left * (player.FacingRight ? 1 : -1), 1f, _stats.GroundLayer);
            var backRay = Physics2D.Raycast(_col.bounds.center - _col.bounds.extents * (player.FacingRight ? 1 : -1), Vector2.down, _col.bounds.extents.y + 1f, _stats.GroundLayer);
            if (!ray || !backRay) return;
            player.LedgePoint = new(ray.point.x, backRay.point.y);
            if (ray.collider.CompareTag("MovingBlock"))
            {
                player.anchorPointBehaviour.SetTarget(ray.transform);
            }
            player.FlipPlayer();
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

    private void ResetJumpBuff()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
    }
    private void ExecuteJump()
    {
        ResetJumpBuff();
        _rb.velocity = new(_rb.velocity.x, _stats.JumpPower);
    }

    private void CheckForBounced(PlayerController player)
    {
        if (player.Bounced)
        {
            if (_grounded && !player.ShieldPushed) player.StopBounceTimer();
            /*else if (_rb.velocity.x < .5f)
            {
                // Buff on y
                var ray = Physics2D.CapsuleCast((Vector2)_col.bounds.center + Vector2.up * _rb.velocity * (_stats.BouncedBuffTime * Time.fixedDeltaTime), _col.size, _col.direction, 0, Vector2.right * Mathf.Sign(_rb.velocity.x), _stats.WallerDistance, _stats.GroundLayer);
                if (!ray)
                {
                    // Able to buff
                    _rb.position += Vector2.up * _rb.velocity * (_stats.BouncedBuffTime * Time.fixedDeltaTime);
                    _rb.velocity += Vector2.right * _preBouncedVelocity;
                }else player.StopBounceTimer();
            }
            else if (_rb.velocity.y < .5f)
            {
                // Buff on x
                var ray = Physics2D.CapsuleCast((Vector2)_col.bounds.center + Vector2.right * _rb.velocity * (_stats.BouncedBuffTime * Time.fixedDeltaTime), _col.size, _col.direction, 0, Vector2.up * Mathf.Sign(_rb.velocity.y), _stats.GrounderDistance, _stats.GroundLayer);
                if (!ray)
                {
                    // Able to buff
                    _rb.position += Vector2.right * _rb.velocity * (_stats.BouncedBuffTime * Time.fixedDeltaTime);
                    _rb.velocity += Vector2.up * _preBouncedVelocity;
                }else player.StopBounceTimer();
            }
            else _preBouncedVelocity = _rb.velocity;*/
        }
    }
    #endregion

    #region Horizontal

    private void HandleDirection(PlayerController player)
    {
        if (((!Mathf.Approximately(Mathf.Sign(FrameInput.x), Mathf.Sign(_rb.velocity.x)) && !Mathf.Approximately(FrameInput.x, 0f)) ||
             Mathf.Approximately(FrameInput.x, 0f)) && player.Bounced)
        {
            player.StopBounceTimer();
        }
        if (FrameInput.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, 0, 
                deceleration * Time.fixedDeltaTime), _rb.velocity.y);
        }
        else if (player.Bounced && Mathf.Abs(_rb.velocity.x) > _stats.MaxBouncedSpeed && 
                 Mathf.Approximately(Mathf.Sign(FrameInput.x), Mathf.Sign(_rb.velocity.x))) 
        {
            // Speed higher than bounced max speed, direct constrain
            _rb.velocity = new(FrameInput.x * _stats.MaxBouncedSpeed, _rb.velocity.y);
        }
        else if (!player.Bounced && Mathf.Abs(_rb.velocity.x) > _stats.MaxSpeed &&
                 Mathf.Approximately(Mathf.Sign(FrameInput.x), Mathf.Sign(_rb.velocity.x)))
        {
            // Speed higher than max speed, direct constrain
            _rb.velocity = new(FrameInput.x * _stats.MaxSpeed, _rb.velocity.y);
        }
        else if(!( Mathf.Abs(_rb.velocity.x) < _stats.MaxBouncedSpeed && Mathf.Abs(_rb.velocity.x) > _stats.MaxSpeed && player.Bounced))
        {
            // Normal
            _rb.velocity = new(Mathf.MoveTowards(_rb.velocity.x, FrameInput.x * _stats.MaxSpeed,
                (player.ShieldPushed ? _stats.PushAcceleration * _stats.Acceleration : _stats.Acceleration) * Time.fixedDeltaTime), _rb.velocity.y);
        }

        // Player rotation
        if (FrameInput.x != 0 && player.FacingRight ^ (_rb.velocity.x > 0))
        {
            player.FlipPlayer(_rb.velocity.x > 0);
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
}

public class PlayerCrouchState : PlayerBaseState
{
    private Rigidbody2D _rd;
    private PlayerStats _stats;
    private CapsuleCollider2D _col;
    private bool _grounded;
    private bool _jumpDown;
    private bool _jumpHeld;
    private Vector2 _move;
    public override void EnterState(PlayerController player)
    {
        _rd = player.Rb;
        _stats = player.stats;
        _col = player.GetComponent<CapsuleCollider2D>();
        _jumpHeld = _jumpDown = false;
        _move = Vector2.zero;
    }

    public override void UpdateState(PlayerController player)
    {
        GatherInput();
        if (_jumpDown)
        {
            ExecuteJump();
            player.SwitchState(Enums.PlayerState.Normal);
            return;
        }
        if (_move.y >= 0)
        {
            player.SwitchState(Enums.PlayerState.Normal);
            return;
        }
    }

    private void GatherInput()
    {
        _jumpDown = Input.GetButtonDown("Jump");
        _jumpHeld = Input.GetButton("Jump");
        _move = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        
    }
    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollisions(player);
        HandleGravity(player);
        
        var decel = _grounded ? _stats.GroundDeceleration : 0f;
        _rd.velocity = new (Mathf.MoveTowards(_rd.velocity.x, 0, decel * Time.fixedDeltaTime), _rd.velocity.y);
    }
    private void ExecuteJump()
    {
        _rd.velocity = new(_rd.velocity.x, _stats.JumpPower);
    }
    private void CheckCollisions(PlayerController player)
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        var groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        var ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);
        var crashCol = player.IsCrashed();
        
        if(crashCol)
        {
            player.ReturnToSpawn();
            return;
        }
        // Hit a Ceiling
        if (ceilingHit && !ceilingHit.collider.isTrigger)
        {
            _rd.velocity = new(_rd.velocity.x, Mathf.Min(0, _rd.velocity.y));
        }

        // Landed on the Ground
        if (!_grounded && groundHit && !groundHit.collider.isTrigger)
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
}
public class PlayerDefenseState : PlayerBaseState
{
    private Rigidbody2D _rd;
    private PlayerStats _stats;
    private CapsuleCollider2D _col;
    private bool _grounded;
    private bool _jumpHeld;
    PlayerStatsManager _statsManager;
    private float _shieldCoolDownTimer;
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Player Defense!");
        _rd = player.Rb;
        _stats = player.stats;
        _col = player.GetComponent<CapsuleCollider2D>();
        _statsManager = PlayerStatsManager.Instance;
        _shieldCoolDownTimer = 0f;
        player.playerHarmable.Shielded = true;
        player.shieldModel.OnDefence = true;
    }

    public override void UpdateState(PlayerController player)
    {
        GatherInput();
        HandleTimer();
        HandleShielded(player);
    }
    private void HandleShielded(PlayerController player)
    {
        if (player.shieldModel == null) return;
        if (player.shieldModel.IsDead)
        {
            player.SwitchState(Enums.PlayerState.Normal);
        }
    }
    private void GatherInput()
    {
        _jumpHeld = Input.GetButton("Jump");
    }

    void HandleTimer()
    {
        _shieldCoolDownTimer -= Time.deltaTime;
    }
    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollisions(player);
        HandleGravity(player);
        
        var decel = _grounded ? _stats.GroundDeceleration : 0f;
        _rd.velocity = new (Mathf.MoveTowards(_rd.velocity.x, 0, decel * Time.fixedDeltaTime), _rd.velocity.y);
    }

    private void CheckCollisions(PlayerController player)
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        var groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        var ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);
        var crashCol = player.IsCrashed();
        
        if(crashCol)
        {
            player.ReturnToSpawn();
            return;
        }
        // Hit a Ceiling
        if (ceilingHit && !ceilingHit.collider.isTrigger)
        {
            _rd.velocity = new(_rd.velocity.x, Mathf.Min(0, _rd.velocity.y));
        }

        // Landed on the Ground
        if (!_grounded && groundHit && !groundHit.collider.isTrigger)
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

    public override void HarmState(PlayerController player, float damage, Vector2 knockback)
    {
        if (_shieldCoolDownTimer > 0f) return;
        _statsManager.ShieldHealth -= damage;
        _shieldCoolDownTimer = .2f;
        if (_statsManager.ShieldHealth <= 0f)
        {
            player.SwitchState(Enums.PlayerState.Normal);
        }
    }

    public override void ExitState(PlayerController player)
    {
        player.playerHarmable.Shielded = false;
        player.shieldModel.OnDefence = false;
    }
}

public class PlayerLedgeState : PlayerBaseState
{
    private Rigidbody2D _rb;
    private float _jumpTimer;
    private float _releaseTimer;
    private Vector2 _move;
    private AnchorPoint _anchorPoint;
    private Vector2 _playerLedgePoint;
    private Vector2 LedgePoint => _playerLedgePoint + (Vector2)_anchorPoint.transform.position - _initAnchorPoint;
    Vector2 _initAnchorPoint;
    PlayerStats _stats;
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Climb Edge");
        
        _rb = player.Rb;
        _stats = player.stats;
        _jumpTimer = 0f;
        _releaseTimer = 0f;
        _anchorPoint = player.anchorPointBehaviour;
        _playerLedgePoint = player.LedgePoint;
        _initAnchorPoint = _anchorPoint.transform.position;
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
        
        _releaseTimer = Mathf.Max(0f, _releaseTimer - Time.deltaTime);
        if (Input.GetButtonDown("Ledge"))
        {
            _releaseTimer = _stats.JumpBuffer;
        }

        _move = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_stats.SnapInput)
        {
            _move.x = Mathf.Abs(_move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_move.x);
            _move.y = Mathf.Abs(_move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_move.y);
        }
    }
    public override void FixedUpdateState(PlayerController player)
    {
        CheckCollision(player);
        HandlePosition(player);
        HandleJump(player);
        ReleaseGrip(player);
    }

    void CheckCollision(PlayerController player)
    {
        var crashCol = player.IsCrashed();
        
        if(crashCol)
        {
            player.ReturnToSpawn();
            return;
        }
    }
    void HandlePosition(PlayerController player)
    {
        // Fixed to the ledge position
        _rb.velocity = Vector2.zero;
        player.transform.position = new(LedgePoint.x + _rb.position.x - player.rightEdgePoint.position.x, LedgePoint.y + _rb.position.y - player.grabPoint.position.y);
        Debug.DrawRay(LedgePoint, Vector3.down, Color.red);
    }
    void HandleJump(PlayerController player)
    {
        if(_jumpTimer > 0)
        {
            if (_move.y >= 0f)
            {
                float additionalVel = player.AnchorPointVelocity.magnitude;
                if (Mathf.Approximately(_move.x, 0f) || additionalVel > .1f || Mathf.Approximately(Mathf.Sign(_move.x), player.FacingRight ? -1 : 1))
                {
                    _rb.velocity = new(_rb.velocity.x /*+ player.AnchorPointVelocity.x*/, _stats.JumpPower /*+ player.AnchorPointVelocity.y*/);
                }
                else
                {
                    _rb.velocity = new(_rb.velocity.x /*+ player.AnchorPointVelocity.x*/, _stats.ClimbPower /*+ player.AnchorPointVelocity.y*/);
                }
                if (additionalVel > .1f)
                {
                    player.SuperBounce();   
                }
            }
            player.SwitchState(Enums.PlayerState.Normal);
        }
    }
    void ReleaseGrip(PlayerController player)
    {
        if (_releaseTimer > 0)
        {
            //player.AnchorPush();
            player.SwitchState(Enums.PlayerState.Normal);
        }
    }
    public override void ExitState(PlayerController player)
    {
        _anchorPoint.SetTarget(null);
    }
}

public class PlayerRespawnState : PlayerBaseState
{
    private PlayerStats _stats;
    private float _nowTime;
    private Vector2 _stPos;
    private ShieldController _shield;
    public override void EnterState(PlayerController player)
    {
        _stats = player.stats;
        _nowTime = 0f;
        _stPos = player.transform.position;
        _shield = ShieldController.Instance;
        
        player.Spr.sortingOrder = 1;
        _shield.GetComponent<SpriteRenderer>().enabled = false;
        Physics2D.IgnoreLayerCollision(player.gameObject.layer, _stats.GroundLayer, true);
        Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Trigger"), true);
        Physics2D.IgnoreLayerCollision(_shield.gameObject.layer, _stats.GroundLayer, true);
        Physics2D.IgnoreLayerCollision(_shield.gameObject.layer, LayerMask.NameToLayer("Trigger"), true);
    }

    public override void UpdateState(PlayerController player)
    {
        if (!(Vector2.Distance(player.transform.position, player.RespawnPoint) > 0.1f))
        {
            player.SwitchState(Enums.PlayerState.Normal);
            return;
        }
        player.transform.position = BetterLerp.Lerp(_stPos, player.RespawnPoint, _nowTime/2f, BetterLerp.LerpType.Sin);
        _shield.transform.position = player.transform.position;
        
        _nowTime += Time.deltaTime;
    }

    public override void ExitState(PlayerController player)
    {
        player.Col.enabled = true;
        player.Spr.sortingOrder = 0;
        _shield.GetComponent<SpriteRenderer>().enabled = true;
        Physics2D.IgnoreLayerCollision(player.gameObject.layer, _stats.GroundLayer, false);
        Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Trigger"), false);
        Physics2D.IgnoreLayerCollision(_shield.gameObject.layer, _stats.GroundLayer, false);
        Physics2D.IgnoreLayerCollision(_shield.gameObject.layer, LayerMask.NameToLayer("Trigger"), false);
    }
}