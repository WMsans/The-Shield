using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBlock : MonoBehaviour, ITriggerable
{
    [Header("Movements")]
    [SerializeField] Transform target;
    [SerializeField] Transform start;
    [SerializeField] float moveTime;
    [SerializeField] float returnTime;
    [SerializeField] Enums.MovingBlockState startingState;
    [Header("Movement Curve")]
    [SerializeField] BetterLerp.LerpType movementType;
    [SerializeField] bool inversed;
    
    private float _time;
    public Enums.MovingBlockState CurrentState {get; private set;}
    private readonly Dictionary<Enums.MovingBlockState, IMovingBlockAction> _states = new()
    {
        { Enums.MovingBlockState.Idle, new MovingBlockResting() },
        {Enums.MovingBlockState.Dashing, new MovingBlockDashing() }
    };
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
    public void SwitchState(Enums.MovingBlockState state)
    {
        if (_states.ContainsKey(state))
        {
            if(_states[state] != null)
                _states[state].OnExit(this);
        }
        CurrentState = state;
        _states[CurrentState].OnEnter(this);
    }

    void Update()
    {
        _states[CurrentState].OnUpdate(this);
    }

    void FixedUpdate()
    {
        _states[CurrentState].OnFixedUpdate(this);
    }
    #region State Machine
    private interface IMovingBlockAction
    {
        public void OnEnter(MovingBlock movingBlock);
        public void OnUpdate(MovingBlock movingBlock);
        public void OnFixedUpdate(MovingBlock movingBlock);
        public void OnExit(MovingBlock movingBlock);
    }

    private class MovingBlockResting : IMovingBlockAction
    {
        public void OnEnter(MovingBlock movingBlock)
        {
            movingBlock.CurrentVelocity = Vector2.zero;
            // Vibrates
            
        }

        public void OnUpdate(MovingBlock movingBlock)
        {
            
        }

        public void OnFixedUpdate(MovingBlock movingBlock)
        {
            movingBlock._rd.velocity = Vector2.zero;
        }

        public void OnExit(MovingBlock movingBlock)
        {
            
        }
    }

    private class MovingBlockDashing : IMovingBlockAction
    {
        private Vector2 _st;
        private Vector2 _tar;
        private float _time;
        private float _tarTime;
        private BetterLerp.LerpType _lerpType;
        public void OnEnter(MovingBlock movingBlock)
        {
            _st = movingBlock._rd.position;
            _tar = movingBlock.target.position;
            _time = 0;
            _tarTime = movingBlock.moveTime * Vector2.Distance(_st, _tar) / Vector2.Distance(movingBlock.start.position, movingBlock.target.position);
        }

        public void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
        }

        public void OnFixedUpdate(MovingBlock movingBlock)
        {
            var nowPos = BetterLerp.Lerp(_st, _tar, _time / _tarTime, _lerpType);
            movingBlock.CurrentVelocity = nowPos - movingBlock._rd.position;
            movingBlock._rd.position = nowPos;
        }

        public void OnExit(MovingBlock movingBlock)
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
        if(CurrentState == Enums.MovingBlockState.Dashing) SwitchState(Enums.MovingBlockState.Returning);
        else SwitchState(Enums.MovingBlockState.Idle);
    }
}
