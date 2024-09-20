using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private float _CoolDownTimer;
    #endregion
    
    #region input

    private bool fireDown;
    
    #endregion
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Hold State");
        
        _rd = shield.Rd;
        _playerRd = PlayerController.Instance.Rd;
        _CoolDownTimer = shield.stats.CoolDownTime;
    }

    public override void UpdateState(ShieldController shield)
    {
        _CoolDownTimer = Mathf.Max(0f, _CoolDownTimer - Time.deltaTime);
        GatherInput();
        if (fireDown && _CoolDownTimer <= 0f)
        {
            // Launch: go to fly state
            shield.SwitchState(Enums.ShieldState.Flying);
        }
    }

    void GatherInput()
    {
        fireDown = Input.GetButtonDown("Fire1");
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
    private Vector2 MousePos {get {return _cam.ScreenToWorldPoint(Input.mousePosition);}}
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Flying State");
        // Initiate variables
        _stats = shield.stats;
        _player = PlayerController.Instance;
        _playerRd = _player.Rd;
        _rd = shield.Rd;
        _cam = CameraFollower.Instance.Cam;
        _chanceOfChangingDir = _stats.MaxChangeDirection;
        
        var dir = (MousePos - _playerRd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // Move in direction
        ChangeDirection(MousePos);
        // Push player in opposite direction
        if (Mathf.DeltaAngle(rot, 180f) < 30f)
        {
            _playerRd.velocity *= Vector2.right;
        }
        _playerRd.velocity -= dir * _stats.ForceToPlayer;
        _player.Bounced = true;
        
    }

    public override void UpdateState(ShieldController shield)
    {
        
    }

    public override void FixedUpdateState(ShieldController shield)
    {
        if (_rd.IsTouchingLayers(_stats.GroundLayer))
        {
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
                    // 直接返回
                    shield.SwitchState(Enums.ShieldState.Returning);
                }
            }
        }
        else if (Vector2.Distance(_rd.position, _playerRd.position) >= _stats.MaxTargetDistance)
        {
            shield.SwitchState(Enums.ShieldState.Returning);
        }
    }

    bool ChangeDirection()
    {
        // Determine the target
        var nextPoint = new ChangePointFinder(_rd, _playerRd, _stats.MaxTargetDistance, _stats.MaxChangeDirection, _stats.GroundLayer, _stats.TargetLayer).NextPosition();
        if (Vector2.Distance(_rd.position, nextPoint) > _stats.MaxTargetDistance)
            return false;
        // Change to that direction
        ChangeDirection(nextPoint);
        return true;
    }

    void ChangeDirection(Vector2 target)
    {
        // Move in direction
        var dir = (target - _rd.position).normalized;
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rd.velocity = dir * _stats.MaxSpeed;
        _rd.rotation = rot;
    }

    class ChangePointFinder
    {
        private Rigidbody2D _shieldRd;
        private Rigidbody2D _playerRd;
        private LayerMask _groundLayer;
        private LayerMask _targetLayer;
        private Vector2 ShieldPosition => _shieldRd.position;
        private Vector2 PlayerPosition => _playerRd.position;
        private List<ShieldAttractingObject> _shieldAttractingObjects;
        private List<bool> _vis;
        private float _maxTargetDistance;
        private int _maxChangeDirection;

        public ChangePointFinder(Rigidbody2D shieldRd, Rigidbody2D playerRd, float shieldMaxDistance, int maxChangeDirection, LayerMask groundLayer, LayerMask targetLayer)
        {
            _shieldRd = shieldRd;
            _playerRd = playerRd;
            _groundLayer = groundLayer;
            _maxTargetDistance = shieldMaxDistance;
            _targetLayer = targetLayer;
            _maxChangeDirection = maxChangeDirection;
            _vis = new List<bool>();
        }

        public Vector2 NextPosition()
        {
            _shieldAttractingObjects = FindAllShieldAttractingObjects();
            var bestDis = Mathf.Infinity;
            var bestPoint = new Vector2();
            Debug.Log(_shieldAttractingObjects.Count);
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
                if(!_vis[i]) continue;// Not comparing itself
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
        bool CheckReturning(int index)
        {
            Vector2 nowPos = _shieldAttractingObjects[index].transform.position;
            // Check if it can return to the player
            var ray = Physics2D.Raycast(nowPos, (PlayerPosition - nowPos).normalized, Mathf.Infinity, _groundLayer);
            if (ray.collider != null)
            {
                // cannot reach player
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
        
    }
}