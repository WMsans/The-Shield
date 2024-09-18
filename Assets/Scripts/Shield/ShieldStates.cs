using System;
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
        private Vector2 _shieldPosition;
        private List<ShieldAttractingObject> _shieldAttractingObjects;
        private float _maxTargetDistance;


        public Vector2 FindChangePoint(Vector2 shieldPos, int chance, float targetDistance)
        {
            _shieldPosition = shieldPos;
            _maxTargetDistance = targetDistance;
            _shieldAttractingObjects = FindAllShieldAttractingObjects();
            
            for (var i = 0; i < _shieldAttractingObjects.Count; i++)
            {
                if (Vector2.Distance(_shieldPosition, _shieldAttractingObjects[i].transform.position) >
                    _maxTargetDistance) break;
                FindNextChangePoint(i, chance - 1);
            }
        }

        void FindNextChangePoint(int index, int chance)
        {
            var nowPos = _shieldAttractingObjects[index].transform.position;
        }
        List<ShieldAttractingObject> FindAllShieldAttractingObjects()
        {
            IEnumerable<ShieldAttractingObject> objects = Object.FindObjectsOfType<ShieldAttractingObject>();
            // return the sorted list
            return objects.OrderBy(obj => Vector2.Distance(obj.transform.position, _shieldPosition)).ToList();
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