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
    private Collider2D _potentialTarget;
    private float _currentMaxSpeed;
    private Vector2 _currentTarget;
    private Vector2 MousePos => _cam.ScreenToWorldPoint(Input.mousePosition);
    private Vector2 PlayerPos => _playerRd.position;
    private Vector2 ShieldPos => _rd.position;
    private Collider2D _nowGroundCollision;
    private bool _outOfRangeFlag;
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Flying State");
        InitializeVariables(shield);
        InitializeTarget();
        InitializeMovement();
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
        _potentialTarget = null;
        _outOfRangeFlag = false;
    }

    void InitializeMovement()
    {
        _currentTarget = MousePos + (MousePos - _rd.position).normalized * _stats.MaxTargetDistance;
        ChangeDirection(_currentMaxSpeed, _currentTarget);
        // Push player in opposite direction
        var dir = (MousePos - _playerRd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (Mathf.DeltaAngle(rot, 180f) < 30f)
        {
            _playerRd.velocity *= Vector2.right;
        }
        _playerRd.velocity -= dir * _stats.ForceToPlayer;
        _player.Bounced = true;
    }

    void InitializeTarget()
    {
        _currentMaxSpeed = _stats.MaxSpeed;
        // Determine if targeted something and set potential target
        var ray = Physics2D.Raycast(ShieldPos, (MousePos - _playerRd.position).normalized, _stats.MaxTargetDistance, _stats.TargetLayer | _stats.GroundLayer);
        if (ray.collider == null) return;
        if (new ChangePointFinder(_rd, _playerRd, _stats).CheckNextPosition(ray.point, _stats.MaxChangeDirection - 1))
        {
            
            _currentMaxSpeed = _stats.MaxTarSpeed;
            _potentialTarget = ray.collider;
        }
    }
    public override void UpdateState(ShieldController shield)
    {
        
    }

    public override void FixedUpdateState(ShieldController shield)
    {
        
        
        // If out range, return
        if (_outOfRangeFlag && Vector2.Distance(_rd.position, _playerRd.position) < _stats.HandRange)
        {
            shield.SwitchState(Enums.ShieldState.Hold);
        }
        else CheckForChangeDirection(shield); // Check for collision
        if (Vector2.Distance(_rd.position, _playerRd.position) > _stats.HandRange) _outOfRangeFlag = true;
        if (Vector2.Distance(_rd.position, _playerRd.position) >= _stats.MaxTargetDistance)
        {
            shield.SwitchState(Enums.ShieldState.Returning);
        }
        // Move in direction
        if(Vector2.SqrMagnitude(_rd.velocity) < Mathf.Epsilon) shield.SwitchState(Enums.ShieldState.Returning);
        ChangeDirection(_currentMaxSpeed, _currentTarget);
    }

    void CheckForChangeDirection(ShieldController shield)
    {
        var cols = new List<Collider2D>();
        var filter = new ContactFilter2D();
        filter.layerMask = _stats.GroundLayer;
        var n = _rd.GetContacts(filter, cols);
        for (var i = 0; i < n; i++)
        {
            if (_rd.IsTouchingLayers(_stats.GroundLayer) && (_nowGroundCollision == null || _nowGroundCollision != cols[i]) )
            {
                _nowGroundCollision = cols[i];
                // Check for returning
                _chanceOfChangingDir--;
                if (_chanceOfChangingDir <= 1)
                {
                    shield.SwitchState(Enums.ShieldState.Returning);
                }
                else
                {
                    // Change direction
                    if (!ChangeDirection())
                    {
                        // Check for potential target
                        if(_potentialTarget != null) _currentTarget = (Vector2)_potentialTarget.transform.position + ((Vector2)_potentialTarget.transform.position - _rd.position).normalized * _stats.MaxTargetDistance;
                        else shield.SwitchState(Enums.ShieldState.Returning);
                    }
                }
                break;
            }
            
        }
    }
    bool ChangeDirection()
    {
        // Determine the target
        var nextPoint = new ChangePointFinder(_rd, _playerRd, _stats).NextPosition();
        if (Vector2.Distance(_rd.position, nextPoint) > _stats.MaxTargetDistance)
            return false;
        // Change to that direction
        _currentTarget = nextPoint + (nextPoint - _rd.position).normalized * _stats.MaxTargetDistance;
        return true;
    }

    void ChangeDirection(float spd, Vector2 target)
    {
        // Move in direction
        var dir = (target - _rd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rd.velocity = dir * spd;
        _rd.rotation = rot;
    }

    class ChangePointFinder
    {
        private Rigidbody2D _shieldRd;
        private Rigidbody2D _playerRd;
        private LayerMask _groundLayer;
        private LayerMask _targetLayer;
        private LayerMask _playerLayer;
        private Vector2 ShieldPosition => _shieldRd.position;
        private Vector2 PlayerPosition => _playerRd.position;
        private List<ShieldAttractingObject> _shieldAttractingObjects;
        private bool[] _vis;
        private float _maxTargetDistance;
        private int _maxChangeDirection;

        public ChangePointFinder(Rigidbody2D shieldRd, Rigidbody2D playerRd, ShieldStats stats)
        {
            _shieldRd = shieldRd;
            _playerRd = playerRd;
            _groundLayer = stats.GroundLayer;
            _maxTargetDistance = stats.MaxTargetDistance;
            _targetLayer = stats.TargetLayer;
            _playerLayer = stats.PlayerLayer;
            _maxChangeDirection = stats.MaxChangeDirection;
            _vis = new bool[100];
        }

        public Vector2 NextPosition()
        {
            _shieldAttractingObjects = FindAllShieldAttractingObjects();
            var bestDis = Mathf.Infinity;
            var bestPoint = new Vector2();
            
            for (int i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                var shieldAttractingObject = _shieldAttractingObjects[i];
                Vector2 tarPoint = _shieldAttractingObjects[i].transform.position;
                // Check if this thing is reachable
                var ray = Physics2D.Raycast(Vector2.MoveTowards(ShieldPosition, tarPoint, .5f), (tarPoint - ShieldPosition).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                if (ray.collider == null || ray.collider != shieldAttractingObject.Col) continue;
                if (!CheckNextPosition(i, +_maxChangeDirection)) continue;
                
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
                if(_vis[i]) continue;// Not comparing itself
                _vis[i] = true;
                var shieldAttractingObject = _shieldAttractingObjects[i];
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
                if(_vis[i]) continue;// Not comparing itself
                _vis[i] = true;
                var shieldAttractingObject = _shieldAttractingObjects[i];
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