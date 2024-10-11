using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBlock : MonoBehaviour, ITriggerable
{
    [Header("Movements")]
    [SerializeField] Transform target;
    [SerializeField] Transform start;
    [SerializeField] float moveTime;
    [SerializeField] float returnSpd;
    [SerializeField] Enums.MovingBlockState startingState;
    [Header("Movement Curve")]
    [SerializeField] BetterLerp.LerpType movementType;
    [SerializeField] bool inversed;
    
    private float _time;
    public MovingBlockAction CurrentState {get; private set;}
    Rigidbody2D _rd;
    public Vector2 CurrentVelocity { get; private set; }

    void Awake()
    {
        _rd = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        SwitchState(startingState);
    }
    public void SwitchState(MovingBlockAction state)
    {
        if(CurrentState != null)
            CurrentState.OnExit(this);
        CurrentState = state;
        state.OnEnter(this);
    }

    void Update()
    {
        CurrentState.OnUpdate(this);
    }

    void FixedUpdate()
    {
        CurrentState.OnFixedUpdate(this);
    }
    #region State Machine

    public abstract class MovingBlockAction
    {
        public abstract void OnEnter(MovingBlock movingBlock);
        public abstract void OnUpdate(MovingBlock movingBlock);
        public abstract void OnFixedUpdate(MovingBlock movingBlock);
        public abstract void OnExit(MovingBlock movingBlock);
        public static implicit operator MovingBlockAction(Enums.MovingBlockState src)
        {
            switch (src)
            {
                case Enums.MovingBlockState.Idle:
                    return new MovingBlockResting();
                case Enums.MovingBlockState.Dashing:
                    return new MovingBlockDashing();
                case Enums.MovingBlockState.Returning:
                    return new MovingBlockReturning();
            }
            return null;
        }
    }

    private class MovingBlockResting : MovingBlockAction
    {
        private float _time;
        public override void OnEnter(MovingBlock movingBlock)
        {
            movingBlock.CurrentVelocity = Vector2.zero;
            // Vibrates
            
        }

        public override void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
            if (_time > 0.5f)
            {
                movingBlock.CurrentVelocity = Vector2.zero;
            }
        }

        public override void OnFixedUpdate(MovingBlock movingBlock)
        {
            movingBlock._rd.velocity = Vector2.zero;
        }

        public override void OnExit(MovingBlock movingBlock)
        {
            
        }
    }

    private class MovingBlockDashing : MovingBlockAction
    {
        private Vector2 _st;
        private Vector2 _tar;
        private float _time;
        private float _tarTime;
        private BetterLerp.LerpType _lerpType;
        private bool _lerpInversed;
        public override void OnEnter(MovingBlock movingBlock)
        {
            _st = movingBlock._rd.position;
            _tar = movingBlock.target.position;
            _time = 0;
            _tarTime = movingBlock.moveTime * Vector2.Distance(_st, _tar) / Vector2.Distance(movingBlock.start.position, movingBlock.target.position);
            _lerpType = movingBlock.movementType;
            _lerpInversed = movingBlock.inversed;
        }

        public override void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
            if (_time >= _tarTime)
            {
                movingBlock.SwitchState(Enums.MovingBlockState.Returning);
            }
        }

        public override void OnFixedUpdate(MovingBlock movingBlock)
        {
            var nowPos = BetterLerp.Lerp(_st, _tar, _time / _tarTime, _lerpType, _lerpInversed);
            movingBlock.CurrentVelocity = nowPos - movingBlock._rd.position;
            movingBlock._rd.position = nowPos;
        }

        public override void OnExit(MovingBlock movingBlock)
        {
            
        }
    }

    private class MovingBlockReturning : MovingBlockAction
    {
        private Vector2 _st;
        private Vector2 _tar;
        private float _time;
        private float _tarTime;
        private bool _movingBack;
        public override void OnEnter(MovingBlock movingBlock)
        {
            _time = 0f;
            _movingBack = false;
            _tar = movingBlock.start.position;
        }

        public override void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
            if (_time > 0.5f)
            {
                _movingBack = true;
            }
        }

        public override void OnFixedUpdate(MovingBlock movingBlock)
        {
            if (_movingBack)
            {
                var nowPos = Vector2.MoveTowards(movingBlock._rd.position, _tar, movingBlock.returnSpd * Time.fixedDeltaTime);
                movingBlock.CurrentVelocity = nowPos - movingBlock._rd.position;
                movingBlock._rd.position = nowPos;
                if (Mathf.Approximately(Vector2.Distance(nowPos, _tar), 0))
                {
                    movingBlock.SwitchState(Enums.MovingBlockState.Idle);
                }
            }
            else
            {
                _st = movingBlock._rd.position;
            }
        }

        public override void OnExit(MovingBlock movingBlock)
        {
            
        }
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (target != null)
        {
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
        Gizmos.color = Color.red;
        if (start != null)
        {
            Gizmos.DrawWireSphere(start.position, 0.5f);
        }

        if (target != null && start != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start.position, target.position);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (start == null) Debug.LogWarning("Please assign a start point asset to the moving block", this);
        if (target == null) Debug.LogWarning("Please assign a target point asset to the moving block", this);
    }
#endif
    public void OnTrigger()
    {
        SwitchState(Enums.MovingBlockState.Dashing);
    }

    public void OnUnTrigger()
    {
        SwitchState(Enums.MovingBlockState.Returning);
    }
}
