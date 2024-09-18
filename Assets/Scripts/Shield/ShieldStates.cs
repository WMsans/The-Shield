using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Rigidbody2D rd;
    private Rigidbody2D playerRd;
    
    #endregion
    
    #region input

    private bool fireDown;
    
    #endregion
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Hold State");
        
        rd = shield.Rd;
        playerRd = PlayerController.Instance.Rd;
    }

    public override void UpdateState(ShieldController shield)
    {
        GatherInput();
        if (fireDown)
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
        
    }

    public override void LateUpdateState(ShieldController shield)
    {
        shield.transform.position = playerRd.position;
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

        // Move in direction
        var dir = (MousePos - _playerRd.position).normalized;
        _rd.velocity = dir * _stats.MaxSpeed;
        // Push player in opposite direction
        var rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
            // Change direction
            ChangeDirection(shield);
        }
    }

    void ChangeDirection(ShieldController shield)
    {
        _chanceOfChangingDir--;
        if (_chanceOfChangingDir <= 1)
        {
            shield.SwitchState(Enums.ShieldState.Returning);
        }
        // Determine the target
        
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
        private float _maxTargetDistance;

        public ChangePointFinder(Rigidbody2D shieldRd, Rigidbody2D playerRd, float shieldMaxDistance, LayerMask groundLayer)
        {
            _shieldRd = shieldRd;
            _playerRd = playerRd;
            _groundLayer = groundLayer;
            _maxTargetDistance = shieldMaxDistance;
        }

        public Vector2 NextPosition()
        {
            _shieldAttractingObjects = FindAllShieldAttractingObjects();
        }

        private bool CheckNextPosition(int index, int chance)
        {
            Vector2 nowPos = _shieldAttractingObjects[index].transform.position;
            if (chance <= 1)
            {
                // Check if it can return to the player
                var ray = Physics2D.Raycast(nowPos, (PlayerPosition - nowPos).normalized, Mathf.Infinity, _groundLayer);
                if (ray.collider != null)
                {
                    // cannot reach player
                    return false;
                }
                return true;
            }

            var bestDis = Mathf.Infinity;
            var bestPoint = Vector2.zero;
            // Go through every item in the shield attracting objects
            for (var i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                if(i == index) continue;// Not comparing itself
                var shieldAttractingObject = _shieldAttractingObjects[i];
                // Check reachable with ray
                var ray = Physics2D.Raycast(nowPos, ((Vector2)shieldAttractingObject.transform.position - nowPos).normalized, _maxTargetDistance, _groundLayer | _targetLayer);
                if(ray.collider == null || ray.collider != shieldAttractingObject.Col) continue;
                // Check within distance
                if (ray.distance > _maxTargetDistance)
                    continue;
                if (bestDis > ray.distance)
                {
                    if (CheckNextPosition(i, chance - 1)) ;
                }
            }
        }
        public Vector2 NextPosition(Rigidbody2D shieldRd, float shieldMaxDistance)
        {
            _shieldRd = shieldRd;
            _maxTargetDistance = shieldMaxDistance;
            NextPosition();
        }
        private List<ShieldAttractingObject> FindAllShieldAttractingObjects()
        {
            IEnumerable<ShieldAttractingObject> objects = Object.FindObjectsOfType<ShieldAttractingObject>();
            // return the sorted list
            return new(objects);
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
    public override void EnterState(ShieldController shield)
    {
        Debug.Log("Shield: Entered Return State");
    }
    public override void UpdateState(ShieldController shield)
    {
        
    }

    public override void FixedUpdateState(ShieldController shield)
    {
        
    }
    public override void LateUpdateState(ShieldController shield)
    {
        
    }
    public override void ExitState(ShieldController shield)
    {
        
    }
}