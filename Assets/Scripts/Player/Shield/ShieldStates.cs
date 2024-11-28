using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class ShieldBaseState
{
    public virtual void EnterState(ShieldController shield){}
    public virtual void UpdateState(ShieldController shield){}
    public virtual void FixedUpdateState(ShieldController shield){}
    public virtual void LateUpdateState(ShieldController shield){}
    public virtual void ExitState(ShieldController shield){}
}

public class ShieldHoldState : ShieldBaseState
{
    private ShieldStats _stats;
    #region movements
    private Rigidbody2D _rd;
    private PlayerController _player;
    private float _coolDownTimer;
    #endregion
    
    #region input
    private Camera _cam;
    private Vector2 MousePos => _cam.ScreenToWorldPoint(Input.mousePosition);
    private bool _parkourMode;
    
    #endregion
    public override void EnterState(ShieldController shield)
    {
        InitializeVariables(shield);
        
        shield.shieldTrail.Active = false;
    }

    void InitializeVariables(ShieldController shield)
    {
        _rd = shield.Rb;
        _player = PlayerController.Instance;
        _cam = CameraFollower.Instance.Cam;
        _stats = shield.stats;
        if(!shield.DisCoolDown)
            _coolDownTimer = shield.stats.CoolDownTime;
        else
            shield.DisCoolDown = false;
    }
    public override void UpdateState(ShieldController shield)
    {
        _coolDownTimer = Mathf.Max(0f, _coolDownTimer - Time.deltaTime);
        HandleInput(shield);
    }

    private void HandleInput(ShieldController shield)
    {
        if(shield.shieldModel && shield.shieldModel.IsDead) return;
        _parkourMode = Input.GetButton("ParkourMode");
        if (shield.FireDownTimer > 0f && _coolDownTimer <= 0f)
        {
            // Fly or melee attack depending on can melee attack
            shield.SwitchState(CanMeleeAttack() ? Enums.ShieldState.Melee : Enums.ShieldState.Flying);
            shield.FireDownTimer = 0;
        }
        else if (shield.DefenseDownTimer > 0f)
        {
            // Defense
            shield.SwitchState(Enums.ShieldState.Defense);
        }
    }
    private bool CanMeleeAttack()
    {
        if(_parkourMode) return false;
        var cols = new List<Collider2D>();
        var filter = new ContactFilter2D();
        filter.SetLayerMask(_stats.TargetLayer);
        _player.meleeDetector.OverlapCollider(filter, cols);
        return cols.Any(x => x.isTrigger == false);
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        _rd.velocity = Vector2.zero;
    }

    public override void LateUpdateState(ShieldController shield)
    {
        shield.transform.position = Vector2.MoveTowards(shield.transform.position, _player.shieldPoint.position, 120f * Time.deltaTime);
    }
}
public class ShieldFlyingState : ShieldBaseState
{
    private ShieldStats _stats;
    private PlayerController _player;
    PlayerStatsManager _statsManager;
    private Rigidbody2D _rb;
    private Rigidbody2D _playerRd;
    private Camera _cam;
    private int _chanceOfChangingDir;
    private float _maxSpeed;
    private Vector2 _currentTarget;
    private Vector2 MousePos => _cam.ScreenToWorldPoint(Input.mousePosition);
    private Vector2 PlayerPos
    {
        get => _playerRd.position;
        set => _playerRd.position = value;
    }
    private Vector2 ShieldPos
    {
        get => _rb.position;
        set => _rb.position = value;
    }
    private List<Collider2D> _collidedFlags; 
    private Collider2D _nowGroundCollision;
    private bool _outOfRangeFlag;
    private bool _holdingAttack;
    private bool _clickingAttack;
    private float _returnTimer;
    private float _holdTimer;
    public override void EnterState(ShieldController shield)
    {
        InitializeVariables(shield);
        InitializeMovement(shield);
        InitializePosition(shield);
        InitializePlayerMovement(shield);
        InitializeTimer(shield);
        
        _player.playerAnimator.SetTrigger("Attack");
        //TimeManager.Instance?.FrozenTime(.008f, .05f);
        shield.shieldTrail.Active = true;
        
    }

    void InitializeTimer(ShieldController shield)
    {
        _returnTimer = 1f;
        _holdTimer = .1f;
    }
    void InitializePosition(ShieldController shield)
    {
        var nowForward = 0.1f;
        while (nowForward <= 0.8f &&
               Physics2D.OverlapCircle(Vector2.MoveTowards(ShieldPos, _currentTarget, nowForward), _stats.DetectionRadius, _stats.GroundLayer) !=
               null)
        {
            nowForward += 0.1f;
        }
        ShieldPos = Vector2.MoveTowards(ShieldPos, _currentTarget, nowForward);
    }
    void InitializeVariables(ShieldController shield)
    {
        // Initiate variables
        _stats = shield.stats;
        _player = PlayerController.Instance;
        _statsManager = PlayerStatsManager.Instance;
        _playerRd = _player.Rb;
        _rb = shield.Rb;
        _cam = CameraFollower.Instance.Cam;
        _chanceOfChangingDir = _stats.MaxChangeDirection;
        _maxSpeed = _stats.MaxSpeed;
        _nowGroundCollision = null;
        _collidedFlags = new();
        _outOfRangeFlag = false;
        _holdingAttack = false;
    }

    void InitializeMovement(ShieldController shield)
    {
        _currentTarget = MousePos + (MousePos - ShieldPos).normalized * _stats.MaxTargetDistance;
        ChangeDirection(_maxSpeed, _currentTarget);
    }

    void InitializePlayerMovement(ShieldController shield)
    {
        // Push player in opposite direction
        var dir = (MousePos - PlayerPos).normalized;
        PushPlayer(dir, shield);
    }

    void PushPlayer(Vector2 dir, ShieldController shield)
    {
        if (_player.CurrentState == Enums.PlayerState.Crouch) return;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (Mathf.DeltaAngle(rot, 270f) < 30f)
        {
            // If push upward, disable player falling
            _playerRd.velocity *= Vector2.right;
        }
        else if (Mathf.DeltaAngle(rot, 90f) < 30f)
        {
            // Don't push downward
            dir *= Vector2.right;
        }
        
        // Decrease player acceleration
        bool neutralBounced;
        if (Mathf.Approximately(Mathf.Sign(dir.x), Mathf.Sign(_player.pressingHor)) && !Mathf.Approximately(_player.pressingHor, 0f))
            neutralBounced = _player.ShieldPush(dir, new Vector2(_stats.HorizontalOpposeForceToPlayer, _stats.VarticleForceToPlayer));
        else 
            neutralBounced = _player.ShieldPush(dir, new Vector2(_stats.HorizontalForceToPlayer, _stats.VarticleForceToPlayer));
        
        if (neutralBounced)
        {
            shield.DisCoolDown = true;
        }
    }
    public override void UpdateState(ShieldController shield)
    {
        GatherInput();
        HandleTimer();
    }

    void HandleTimer()
    {
        _returnTimer -= Time.deltaTime;
        _holdTimer -= Time.deltaTime;
    }
    void GatherInput()
    {
        _holdingAttack = Input.GetButton("Fire1");
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        // If out range, return
        var dir = (_currentTarget - ShieldPos).normalized;
        if (_outOfRangeFlag && Vector2.Distance(ShieldPos, _player.shieldPoint.position) < _stats.HandRange)
        {
            shield.SwitchState(Enums.ShieldState.Hold);
        }
        else if (shield.FireDownTimer > 0f /*&& Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, 270f)) >= 30f*/ && _returnTimer <= 0f)
        {
            // Return if clicking attack and shield is not flying downward
            shield.SwitchState(Enums.ShieldState.Returning);
            shield.FireDownTimer = 0f;
            return;
        }
        else
        {
            CheckForChangeDirection(shield);// Check for collision
        } 
        if (Vector2.Distance(ShieldPos, PlayerPos) > _stats.HandRange && _holdTimer < 0f) _outOfRangeFlag = true;
        if (Vector2.Distance(ShieldPos, PlayerPos) >= _stats.MaxTargetDistance)
        {
            shield.SwitchState(Enums.ShieldState.Returning);
        }
        // Move in direction
        ChangeDirection(_maxSpeed, _currentTarget);
    }
    void CheckForChangeDirection(ShieldController shield)
    {
        var ray = Physics2D.CircleCastAll(ShieldPos, _stats.DetectionRadius, _rb.velocity.normalized, _maxSpeed, _stats.GroundLayer | _stats.TargetLayer);
        var shakeFlag = false;
        foreach (var i in ray)
        {
            if(!i) continue;
            var t = i.collider;
            if(i.distance > _stats.MaxSpeed * Time.fixedDeltaTime) continue; 
            if(t.isTrigger) continue;
            var realGrounded = (_stats.GroundLayer & (1 << t.gameObject.layer)) != 0;
            if (_nowGroundCollision && _nowGroundCollision == t) continue;
            _nowGroundCollision = t;
            _collidedFlags.Add(t);
            // Check for returning
            _chanceOfChangingDir--;
            // Check for tags
            if (t.CompareTag("Trigger"))
            {
                t.GetComponent<Trigger>().OnTrigger();
            }
            else
            {
                var harmables = t.GetComponents<Harmable>().ToList();
                if(harmables.Count > 0)
                {
                    shakeFlag = true;
                    foreach (var h in harmables)
                    {
                        h.Harm(_statsManager.PlayerDamage /*, (i.point - (Vector2)t.transform.position).normalized*/);
                    }
                }
            }
            if (_chanceOfChangingDir <= 1)
            {
                // No chance, return
                shield.SwitchState(Enums.ShieldState.Returning);
            }
            else if (realGrounded && _holdingAttack)
            {
                // Holding left mouse, no attract, Return
                shield.SwitchState(Enums.ShieldState.Returning);
            }
            else
            {
                // Change direction
                if (!ChangeDirection())
                {
                    shield.SwitchState(Enums.ShieldState.Returning);
                }
            }
        }
        var cols = Physics2D.OverlapCircleAll(ShieldPos, _stats.DetectionRadius, _stats.GroundLayer | _stats.TargetLayer);
        foreach (var t in cols)
        {
            if(!t) continue;
            if(t.isTrigger) continue;
            var realGrounded = (_stats.GroundLayer & (1 << t.gameObject.layer)) != 0;
            if (_nowGroundCollision && _nowGroundCollision == t) continue;
            _nowGroundCollision = t;
            _collidedFlags.Add(t);
            // Check for returning
            _chanceOfChangingDir--;
            if (t.CompareTag("Trigger"))
            {
                t.GetComponent<Trigger>().OnTrigger();
            }
            else
            {
                var harmables = t.GetComponents<Harmable>().ToList();
                if (harmables.Count > 0)
                {
                    shakeFlag = true;
                    foreach (var h in harmables)
                        h.Harm(_statsManager.PlayerDamage, (ShieldPos - (Vector2)t.transform.position).normalized);
                }
            }
            if (_chanceOfChangingDir <= 1)
            {
                // No chance, return
                shield.SwitchState(Enums.ShieldState.Returning);
            }
            else if (realGrounded && _holdingAttack)
            {
                // Holding left mouse, no attract, Return
                shield.SwitchState(Enums.ShieldState.Returning);
            }
            else
            {
                // Change direction
                if (!ChangeDirection())
                {
                    shield.SwitchState(Enums.ShieldState.Returning);
                }
            }
        }

        if (shakeFlag)
        {
            TimeManager.Instance?.FrozenTime(.024f, .05f);
            CameraShake.Instance?.ShakeCamera(0.02f, 0.3f);
        }
    }
    bool ChangeDirection()
    {
        // Determine the target
        var nextPoint = new ChangePointFinder(_rb, _playerRd, _collidedFlags, _stats).NextPosition();
        if (Vector2.Distance(ShieldPos, nextPoint) > _stats.MaxTargetDistance)
            return false;
        // Change to that direction
        _currentTarget = nextPoint + (nextPoint - ShieldPos).normalized * _stats.MaxTargetDistance;
        return true;
    }

    void ChangeDirection(float spd, Vector2 target)
    {
        // Move in direction
        var dir = (target - ShieldPos).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rb.velocity = dir * spd;
        _rb.rotation = rot;
    }
    private List<ShieldAttractingObject> FindAllShieldAttractingObjects()
    {
        IEnumerable<ShieldAttractingObject> objects = Object.FindObjectsOfType<ShieldAttractingObject>();
        // return the sorted list
        return new(objects);
    }
    class ChangePointFinder
    {
        private readonly Rigidbody2D _shieldRd;
        private readonly Rigidbody2D _playerRd;
        private readonly LayerMask _groundLayer;
        private readonly LayerMask _targetLayer;
        private readonly LayerMask _playerLayer;
        private readonly ShieldStats _stats;
        private Vector2 ShieldPosition => _shieldRd.position;
        private Vector2 PlayerPosition => _playerRd.position;
        private List<ShieldAttractingObject> _shieldAttractingObjects;
        private bool[] _vis;
        private List<Collider2D> _collidedFlags;
        private readonly float _maxTargetDistance;
        private readonly int _maxChangeDirection;

        public ChangePointFinder(Rigidbody2D shieldRd, Rigidbody2D playerRd, List<Collider2D> collidedFlags, ShieldStats stats)
        {
            _stats = stats;
            _shieldRd = shieldRd;
            _playerRd = playerRd;
            _groundLayer = stats.GroundLayer;
            _maxTargetDistance = stats.MaxTargetDistance;
            _targetLayer = stats.TargetLayer;
            _playerLayer = stats.PlayerLayer;
            _maxChangeDirection = stats.MaxChangeDirection;
            _vis = new bool[100];
            _collidedFlags = collidedFlags;
        }

        public Vector2 NextPosition()
        {
            _shieldAttractingObjects = FindAllShieldAttractingObjects();
            var bestDis = Mathf.Infinity;
            var bestPoint = new Vector2();
            
            foreach (var t in _shieldAttractingObjects)
            {
                var shieldAttractingObject = t;
                if(_collidedFlags.Contains(shieldAttractingObject.Col)) continue;
                Vector2 tarPoint = t.transform.position;
                // Check if this thing is reachable
                var ray = Physics2D.Raycast(ShieldPosition, (tarPoint - ShieldPosition).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                if (ray.collider == null || ray.transform != shieldAttractingObject.transform || ray.distance > shieldAttractingObject.AttractDistance) continue;
                // Check if it is best distance
                if(ray.distance < bestDis)
                {
                    bestDis = ray.distance;
                    bestPoint = ray.point;
                }
            }
            if(bestDis < _maxTargetDistance)
                return bestPoint;
            return new(Mathf.Infinity, Mathf.Infinity);
        }

        public bool CheckNextPosition(Vector2 nextPoint, int chance)
        {
            Vector2 nowPos = nextPoint;
            if (chance <= 1)
            {
                return false;
            }
            // Go through every point in the shield attracting objects
            if(_shieldAttractingObjects == null) _shieldAttractingObjects = FindAllShieldAttractingObjects();
            for (var i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                var shieldAttractingObject = _shieldAttractingObjects[i];
                if(_vis[i]) continue;// Don't bounce twice
                _vis[i] = true;
                
                // Check reachable with ray
                var ray = Physics2D.Raycast(nowPos, ((Vector2)shieldAttractingObject.transform.position - nowPos).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                
                if(ray.collider == null || ray.collider != shieldAttractingObject.Col) continue;
                // Check within distance
                // Check within distance
                if (ray.distance > _maxTargetDistance) continue;
                // Check from this point
                if (CheckNextPosition(i, chance - 1))
                {
                    return true;
                }
            }
            
            // No object is available to go
            return false;
        }
        private bool CheckNextPosition(int index, int chance)
        {
            Vector2 nowPos = _shieldAttractingObjects[index].transform.position;
            if (chance <= 1)
            {
                return CheckReturning(index);
            }
            _vis[index] = true;
            // Go through every point in the shield attracting objects
            for (var i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                var shieldAttractingObject = _shieldAttractingObjects[i];
                if(_vis[i]) continue;
                _vis[i] = true;
                
                // Check reachable with ray
                var ray = Physics2D.Raycast(nowPos, ((Vector2)shieldAttractingObject.transform.position - nowPos).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                if(ray.collider == null || ray.collider != shieldAttractingObject.Col) continue;
                // Check within distance
                if (ray.distance > _maxTargetDistance) continue;
                // Check from this point
                
                if (CheckNextPosition(i, chance - 1))
                {
                    return true;
                }
            }
            
            return CheckReturning(index);
        }
        
        private List<ShieldAttractingObject> FindAllShieldAttractingObjects()
        {
            IEnumerable<ShieldAttractingObject> objects = Object.FindObjectsOfType<ShieldAttractingObject>();
            // return the sorted list
            return new(objects);
        }
        bool CheckReturning(Vector2 nowPos)
        {
            // Check if it can return to the player
            var ray = Physics2D.Raycast(Vector2.MoveTowards(nowPos, PlayerPosition, 1f), (PlayerPosition - nowPos).normalized, Mathf.Infinity, _groundLayer | _playerLayer);
            if (ray.collider != null)
            {
                // reaches player
                if(ray.collider.CompareTag("Player")) return true;
                return false;
            }
            return true;
        }
        bool CheckReturning(int index)
        {
            Vector2 nowPos = _shieldAttractingObjects[index].transform.position;
            // Check if it can return to the player
            var ray = Physics2D.Raycast(Vector2.MoveTowards(nowPos, PlayerPosition, 1f), (PlayerPosition - nowPos).normalized, Mathf.Infinity, _groundLayer | _playerLayer);
            if (ray.collider != null)
            {
                // reaches player
                if(ray.collider.CompareTag("Player")) return true;
                return false;
            }
            return true;
        }
    }
    public override void ExitState(ShieldController shield)
    {
        
    }
}
public class ShieldReturnState : ShieldBaseState
{
    Rigidbody2D _rd;
    Rigidbody2D _playerRd;
    ShieldStats _stats;
    Collider2D _col;
    List<Collider2D> _colList;
    public override void EnterState(ShieldController shield)
    {
        _rd = shield.Rb;
        _playerRd = PlayerController.Instance.Rb;
        _stats = shield.stats;

        _col = shield.GetComponent<Collider2D>();
        _col.enabled = false;
        _colList = new();
    }
    void ChangeDirection(Vector2 target)
    {
        // Move in direction
        var dir = (target - _rd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rd.velocity = dir * _stats.MaxSpeed;
        _rd.rotation = rot;
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        ChangeDirection(_playerRd.position);
        CheckForTarget(shield);
        if (Vector2.Distance(_rd.position, _playerRd.position) < _stats.HandRange)
        {
            shield.SwitchState(Enums.ShieldState.Hold);
        }
    }

    private void CheckForTarget(ShieldController shield)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(_stats.TargetLayer);
        var cols = Physics2D.OverlapCircleAll(_rd.position, _stats.DetectionRadius, _stats.TargetLayer);
        foreach (var c in cols)
        {
            if(_colList.Contains(c)) continue;
            c.GetComponent<Harmable>()?.Harm(PlayerStatsManager.Instance.PlayerDamage);
            _colList.Add(c);
        }
    }
    public override void ExitState(ShieldController shield)
    {
        shield.GetComponent<Collider2D>().enabled = true;
    }
}

public class ShieldMeleeState : ShieldBaseState
{
    private static readonly int Melee = Animator.StringToHash("Melee");
    PlayerController _player;
    private float _debugTimer;
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield Melee Attack!!!");
        _player = PlayerController.Instance;
        _player.playerAnimator.SetTrigger(Melee);
        shield.shieldTrail.Active = false;
    }

    public override void UpdateState(ShieldController shield)
    {
        _debugTimer += Time.deltaTime;
        if (_debugTimer >= 0.5f)
        {
            if(shield.FireDownTimer > 0) _debugTimer = 0;
            else shield.SwitchState(Enums.ShieldState.Hold);
        }
    }

    public override void FixedUpdateState(ShieldController shield)
    {
        
    }

    public override void LateUpdateState(ShieldController shield)
    {
        shield.transform.position = Vector2.MoveTowards(shield.transform.position, _player.shieldPoint.position, 120f * Time.deltaTime);
    }

    public override void ExitState(ShieldController shield)
    {
        _player.playerAnimator.SetBool(Melee, false);
    }
}

public class ShieldDefenseState : ShieldBaseState
{
    private PlayerController _player;
    private bool _defenseButton;
    private Rigidbody2D _rb;
    public override void EnterState(ShieldController shield)
    {
        _player = PlayerController.Instance;
        if(_player.CurrentState != Enums.PlayerState.Ledge)
            _player.SwitchState(Enums.PlayerState.Defense);

        _rb = shield.Rb;
        shield.shieldTrail.Active = true;
    }

    public override void UpdateState(ShieldController shield)
    {
        GatherInput();
    }

    private void GatherInput()
    {
        _defenseButton = Input.GetButton("Defense");
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        _rb.velocity = Vector2.zero;
        
        if (!_defenseButton)
        {
            // Return to the normal state
            shield.SwitchState(Enums.ShieldState.Hold);
        }
    }

    public override void LateUpdateState(ShieldController shield)
    {
        shield.transform.position = Vector2.MoveTowards(shield.transform.position, _player.shieldPoint.position, 120f * Time.deltaTime);
    }

    public override void ExitState(ShieldController shield)
    {
        if(_player.CurrentState != Enums.PlayerState.Ledge)
            _player.SwitchState(Enums.PlayerState.Normal);
    }
}

