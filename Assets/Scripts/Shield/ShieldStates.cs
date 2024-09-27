using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class ShieldBaseState
{
    public abstract void EnterState(ShieldController shield);
    public abstract void UpdateState(ShieldController shield);
    public abstract void FixedUpdateState(ShieldController shield);
    public abstract void LateUpdateState(ShieldController shield);
    public abstract void ExitState(ShieldController shield);
}

public class ShieldHoldState : ShieldBaseState
{
    #region movements
    private Rigidbody2D _rd;
    private Rigidbody2D _playerRd;
    private float _coolDownTimer;
    #endregion
    
    #region input

    private bool _fireDown;
    
    #endregion
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Hold State");
        
        _rd = shield.Rd;
        _playerRd = PlayerController.Instance.Rd;
        _coolDownTimer = shield.stats.CoolDownTime;
    }

    public override void UpdateState(ShieldController shield)
    {
        _coolDownTimer = Mathf.Max(0f, _coolDownTimer - Time.deltaTime);
        GatherInput();
        if (_fireDown && _coolDownTimer <= 0f)
        {
            // Launch: go to fly state
            shield.SwitchState(Enums.ShieldState.Flying);
        }
    }

    void GatherInput()
    {
        _fireDown = Input.GetButtonDown("Fire1");
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        _rd.velocity = Vector2.zero;    
    }

    public override void LateUpdateState(ShieldController shield)
    {
        shield.transform.position = Vector2.MoveTowards(shield.transform.position, _playerRd.position, 120f * Time.deltaTime);
    }
    public override void ExitState(ShieldController shield)
    {
        
    }
}
public class ShieldFlyingState : ShieldBaseState
{
    private ShieldStats _stats;
    PlayerController _player;
    private Rigidbody2D _rd;
    private Rigidbody2D _playerRd;
    private Camera _cam;
    private int _chanceOfChangingDir;
    private float _currentMaxSpeed;
    private Vector2 _currentTarget;
    private Vector2 MousePos => _cam.ScreenToWorldPoint(Input.mousePosition);
    private Vector2 PlayerPos => _playerRd.position;
    private Vector2 ShieldPos => _rd.position;
    private List<Collider2D> _collidedFlags; 
    private Collider2D _nowGroundCollision;
    private bool _outOfRangeFlag;
    private bool _holdingAttack;
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Flying State");
        InitializeVariables(shield);
        InitializeMovement();
        InitializePlayerMovement();
    }

    void InitializeVariables(ShieldController shield)
    {
        // Initiate variables
        _stats = shield.stats;
        _player = PlayerController.Instance;
        _playerRd = _player.Rd;
        _rd = shield.Rd;
        _cam = CameraFollower.Instance.Cam;
        _chanceOfChangingDir = _stats.MaxChangeDirection;
        _currentMaxSpeed = _stats.MaxSpeed;
        _nowGroundCollision = null;
        _collidedFlags = new();
        _outOfRangeFlag = false;
        _holdingAttack = false;
    }

    void InitializeMovement()
    {
        _currentTarget = MousePos + (MousePos - ShieldPos).normalized * _stats.MaxTargetDistance;
        ChangeDirection(_currentMaxSpeed, _currentTarget);
    }

    void InitializePlayerMovement()
    {
        // Push player in opposite direction
        var dir = (MousePos - PlayerPos).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (Mathf.DeltaAngle(rot, 180f) < 30f)
        {
            // If push upward, disable player falling
            _playerRd.velocity *= Vector2.right;
        }
        else if (Mathf.DeltaAngle(rot, 0f) < 30f)
        {
            // Don't push downward
            dir = Vector2.zero;
        }
        _playerRd.velocity -= dir * _stats.ForceToPlayer;
        _player.Bounced = true;
        _player.StartBounceTimer();
        // Decrease player acceleration
        _player.ShieldPush();
    }
    public override void UpdateState(ShieldController shield)
    {
        GatherInput();
    }

    void GatherInput()
    {
        _holdingAttack = Input.GetButton("Fire1");
    }
    public override void FixedUpdateState(ShieldController shield)
    {
        // If out range, return
        if (_outOfRangeFlag && Vector2.Distance(ShieldPos, PlayerPos) < _stats.HandRange)
        {
            shield.SwitchState(Enums.ShieldState.Hold);
        }
        else CheckForChangeDirection(shield); // Check for collision
        if (Vector2.Distance(ShieldPos, PlayerPos) > _stats.HandRange) _outOfRangeFlag = true;
        if (Vector2.Distance(ShieldPos, PlayerPos) >= _stats.MaxTargetDistance)
        {
            shield.SwitchState(Enums.ShieldState.Returning);
        }
        // Move in direction
        ChangeDirection(_currentMaxSpeed, _currentTarget);
    }
    void CheckForChangeDirection(ShieldController shield)
    {
        var cols = Physics2D.OverlapCircleAll(ShieldPos, _stats.DetectionRadius, _stats.GroundLayer | _stats.TargetLayer);
        
        foreach (var t in cols)
        {
            var realGrounded = (_stats.GroundLayer & (1 << t.gameObject.layer)) != 0;
            if (_nowGroundCollision == null || _nowGroundCollision != t)
            {
                _nowGroundCollision = t;
                _collidedFlags.Add(t);
                // Check for returning
                _chanceOfChangingDir--;
                if (_chanceOfChangingDir <= 1)
                {
                    shield.SwitchState(Enums.ShieldState.Returning);
                }
                else if (!realGrounded && _holdingAttack)
                {
                    // Fly as well
                    
                }
                else if (realGrounded && _holdingAttack)
                {
                    // Return
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
                
                break;
            }
        }
    }
    bool ChangeDirection()
    {
        // Determine the target
        var nextPoint = new ChangePointFinder(_rd, _playerRd, _collidedFlags, _stats).NextPosition();
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
        _rd.velocity = dir * spd;
        _rd.rotation = rot;
    }
    class ChangePointFinder
    {
        private readonly Rigidbody2D _shieldRd;
        private readonly Rigidbody2D _playerRd;
        private readonly LayerMask _groundLayer;
        private readonly LayerMask _targetLayer;
        private readonly LayerMask _playerLayer;
        private Vector2 ShieldPosition => _shieldRd.position;
        private Vector2 PlayerPosition => _playerRd.position;
        private List<ShieldAttractingObject> _shieldAttractingObjects;
        private bool[] _vis;
        private List<Collider2D> _collidedFlags;
        private readonly float _maxTargetDistance;
        private readonly int _maxChangeDirection;

        public ChangePointFinder(Rigidbody2D shieldRd, Rigidbody2D playerRd, List<Collider2D> collidedFlags, ShieldStats stats)
        {
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
            
            for (int i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                var shieldAttractingObject = _shieldAttractingObjects[i];
                if(_collidedFlags.Contains(shieldAttractingObject.Col)) continue;
                Vector2 tarPoint = _shieldAttractingObjects[i].transform.position;
                // Check if this thing is reachable
                var ray = Physics2D.Raycast(ShieldPosition, (tarPoint - ShieldPosition).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                if (ray.collider == null || ray.transform != shieldAttractingObject.transform) continue;
                if (!CheckNextPosition(i, _maxChangeDirection)) continue;
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
    
    
    public override void LateUpdateState(ShieldController shield)
    {
        
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
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Return State");
        
        _rd = shield.Rd;
        _playerRd = PlayerController.Instance.Rd;
        _stats = shield.stats;

        shield.GetComponent<Collider2D>().enabled = false;
    }
    void ChangeDirection(Vector2 target)
    {
        // Move in direction
        var dir = (target - _rd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rd.velocity = dir * _stats.MaxSpeed;
        _rd.rotation = rot;
    }
    public override void UpdateState(ShieldController shield)
    {
        
    }

    public override void FixedUpdateState(ShieldController shield)
    {
        ChangeDirection(_playerRd.position);
        if (Vector2.Distance(_rd.position, _playerRd.position) < _stats.HandRange)
        {
            shield.SwitchState(Enums.ShieldState.Hold);
        }
    }
    public override void LateUpdateState(ShieldController shield)
    {
        
    }
    public override void ExitState(ShieldController shield)
    {
        shield.GetComponent<Collider2D>().enabled = true;
    }
}