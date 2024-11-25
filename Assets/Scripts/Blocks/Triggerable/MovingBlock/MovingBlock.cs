using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBlock : MonoBehaviour, ITriggerable, IPersistant
{
    [Header("Movements")]
    [SerializeField] Transform target;
    [SerializeField] Transform start;
    [SerializeField] float moveTime;
    [SerializeField] float returnSpd;
    [SerializeField] Enums.MovingBlockState startingState;
    [SerializeField] bool autoReturn = true;
    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask obstacleLayer;
    [Header("Reset")]
    [SerializeField] bool persistant;
    [SerializeField]private string _id;
    [Header("Movement Curve")]
    [SerializeField] BetterLerp.LerpType movementType;
    [SerializeField] bool inversed;
    Vector3 _oriPosition;
    private float _time;
    public MovingBlockAction CurrentState {get; private set;}
    Rigidbody2D _rb;
    private PlayerController _player;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        _player = PlayerController.Instance;
        SwitchState(startingState);
    }
    public void SwitchState(MovingBlockAction state)
    {
        CurrentState?.OnExit(this);
        CurrentState = state;
        CurrentState?.OnEnter(this);
    }

    void Update()
    {
        _time += Time.deltaTime;
        if (PlayerExiting)
        {
            _player.transform.parent = null;
            SceneManager.MoveGameObjectToScene(_player.gameObject, SceneManager.GetSceneByName("PersistantScene"));
            if(_player.anchorPointBehaviour.Target == transform)
                _player.anchorPointBehaviour.SetTarget(null);
            _player.AnchorPush();
            _playerExitTime = 0f;
        }
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
            return src switch
            {
                Enums.MovingBlockState.Idle => new MovingBlockResting(),
                Enums.MovingBlockState.Dashing => new MovingBlockDashing(),
                Enums.MovingBlockState.Returning => new MovingBlockReturning(),
                _ => null
            };
        }
    }

    private class MovingBlockResting : MovingBlockAction
    {
        private float _time;
        public override void OnEnter(MovingBlock movingBlock)
        {
            // Vibrates
            
        }

        public override void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
        }

        public override void OnFixedUpdate(MovingBlock movingBlock)
        {
            movingBlock._rb.velocity = Vector2.zero;
        }

        public override void OnExit(MovingBlock movingBlock)
        {
            
        }
    }

    private class MovingBlockDashing : MovingBlockAction
    {
        private Vector2 _st;
        private Vector2 _tar;
        private Bounds _colBound;
        private float _time;
        private float _tarTime;
        private BetterLerp.LerpType _lerpType;
        private bool _lerpInversed;
        public override void OnEnter(MovingBlock movingBlock)
        {
            _st = movingBlock._rb.position;
            _tar = movingBlock.target.position;
            _colBound = movingBlock.gameObject.GetComponentInParent<Collider2D>().bounds;
            _time = 0;
            _tarTime = movingBlock.moveTime * Vector2.Distance(_st, _tar) / Vector2.Distance(movingBlock.start.position, movingBlock.target.position);
            _lerpType = movingBlock.movementType;
            _lerpInversed = movingBlock.inversed;
        }

        public override void OnUpdate(MovingBlock movingBlock)
        {
            _time += Time.deltaTime;
            if (Vector2.Distance(movingBlock.transform.position, movingBlock.target.position) < 0.1f)
            {
                StopDash(movingBlock);
            }
        }

        public override void OnFixedUpdate(MovingBlock movingBlock)
        {
            var nowPos = BetterLerp.Lerp(_st, _tar, _time / _tarTime, _lerpType, _lerpInversed);
            var ray = Physics2D.BoxCast(movingBlock.transform.position, _colBound.size,
                movingBlock.transform.rotation.z, (_tar - _st).normalized, Vector2.Distance(nowPos, movingBlock.transform.position), movingBlock.obstacleLayer);
            if (ray)
            {
                // Move to the block
                movingBlock.transform.position += new Vector3((_tar - _st).normalized.x, (_tar - _st).normalized.y) * ray.distance;
                StopDash(movingBlock);
                return;
            }
            
            movingBlock.transform.position = nowPos;
        }

        void StopDash(MovingBlock movingBlock)
        {
            if(movingBlock.autoReturn)
                movingBlock.SwitchState(Enums.MovingBlockState.Returning);
            else
            {
                movingBlock.SwitchState(Enums.MovingBlockState.Idle);
                (movingBlock.target, movingBlock.start) = (movingBlock.start, movingBlock.target);
            }
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
            movingBlock._rb.velocity = Vector2.zero;
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
                var nowPos = Vector2.MoveTowards(movingBlock.transform.position, _tar, movingBlock.returnSpd * Time.fixedDeltaTime);
                movingBlock.transform.position = nowPos;
                if (Mathf.Approximately(Vector2.Distance(nowPos, _tar), 0))
                {
                    movingBlock.SwitchState(Enums.MovingBlockState.Idle);
                }
            }
            else
            {
                _st = movingBlock.transform.position;
            }
        }

        public override void OnExit(MovingBlock movingBlock)
        {
            
        }
    }
    #endregion

    private float _playerExitTime;
    private bool _initialized;
    
    private bool PlayerExiting => Mathf.Abs(_playerExitTime - _time) < 0.05f;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _player.transform.parent = transform;
            _player.anchorPointBehaviour.SetTarget(transform);
            _playerExitTime = 0f;
        }
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerExitTime = _time + 0.1f;
            
        }
    }

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

    public void OnInitialize()
    {
        LoadData();
        _oriPosition = transform.position;
        _initialized = true;
    }

    public void OnReset()
    {
        if (!_initialized) return;
        if(persistant) return;
        transform.position = _oriPosition;
    }

    private void OnEnable()
    {
        OnInitialize();
    }
    
    public void SaveData()
    {
        if (!persistant || _id.Length < 1) return;
        ES3.Save(_id + "Position", transform.position);
    }

    public void LoadData()
    {
        if (!persistant || _id.Length < 1 || !ES3.FileExists()) return;
        transform.position = ES3.Load(_id + "Position", transform.position);
    }

    void OnDisable()
    {
        SaveData();
    }

    [ContextMenu("Generate Guid")]
    public void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }
}
