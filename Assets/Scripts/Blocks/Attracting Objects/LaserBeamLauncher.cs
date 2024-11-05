using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamLauncher : ShieldAttractingObject, IHarmable, IPersistant
{
    [Header("Laser Beam Properties")]
    [SerializeField] private float distanceRay = 100f;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private LayerMask collidableLayer;
    [SerializeField] private float laserBeamRadius;
    [SerializeField] private float launchPeriod;
    [SerializeField] private float cooldownPeriod;
    [Header("Harmable Properties")]
    [SerializeField] private bool harmable = true;
    [SerializeField] private float hpMax;
    [SerializeField] private bool persistant;
    private bool _launched;
    [Header("Player Interaction")]
    [SerializeField] private float attractDis = 100f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool forceRespawn;
    private float _forceRespawnTimer = 0f;
    private float _launchTimer = 0f;
    LineRenderer _laserBeamRenderer;
    private PlayerController _player;
    private float _hitPoints;
    private string _id;
    private float _realCooldownPeriod;
    public override float AttractDistance => attractDis;

    protected override void Awake()
    {
        base.Awake();
        _laserBeamRenderer = GetComponent<LineRenderer>();
        _realCooldownPeriod = cooldownPeriod;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _launched = false;
    }

    private void Update()
    {
        ShootLaser();
        HandleTimer();
    }

    void ShootLaser()
    {
        if(!_launched)
        {
            _laserBeamRenderer.enabled = false;
            return;
        }
        _laserBeamRenderer.enabled = true;
        var ray = Physics2D.Raycast(laserPoint.position, transform.right, Mathf.Infinity, collidableLayer);
        if (ray)
        {
            Render2DRay(laserPoint.position, ray.point, laserBeamRadius);
        }
        else
        {
            Render2DRay(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay, laserBeamRadius);
        }
        HarmableDetection(Physics2D.CircleCast(laserPoint.position, laserBeamRadius, transform.right, Mathf.Infinity, collidableLayer));
    }

    void HandleTimer()
    {
        _launchTimer += Time.deltaTime;
        if (_launchTimer >= (_launched ? launchPeriod : _realCooldownPeriod))
        {
            _launched = !_launched;
            _launchTimer = 0f;
        }
    }
    void HarmableDetection(RaycastHit2D ray)
    {
        if(!ray) return;
        if (ray.collider.CompareTag("Player"))
        {
            _player.Harm(damage);
            // Player detected, harm player
            if (forceRespawn && _forceRespawnTimer <= 0f)
            {
                StartCoroutine(ForceRespawn());
            }
        }
    }

    IEnumerator ForceRespawn()
    {
        _player.ReturnToSpawn();
        _forceRespawnTimer = 1f;
        while (_forceRespawnTimer > 0)
        {
            _forceRespawnTimer -= Time.deltaTime;
            yield return null;
        }
    }
    public override void OnInitialize()
    {
        base.OnInitialize();
        LoadData();
    }
    public override void OnReset()
    {
        if(persistant) return;
        base.OnReset();
        _launched = true;
        _forceRespawnTimer = 0f;
        _launchTimer = 0f;
        _realCooldownPeriod = cooldownPeriod;
        _hitPoints = hpMax;
    }
    void Render2DRay(Vector2 startPos, Vector2 endPos, float width)
    {
        _laserBeamRenderer.SetPosition(0, startPos);
        _laserBeamRenderer.SetPosition(1, endPos);
        _laserBeamRenderer.startWidth = width + .1f;
        _laserBeamRenderer.endWidth = width + .25f;
    }
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(laserPoint) Gizmos.DrawLine(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay);    
    }
    #endif
    float IHarmable.HitPoints
    {
        get => _hitPoints;
        set => _hitPoints = value;
    }

    public void Die()
    {
        if(!harmable) return;
        _realCooldownPeriod = Mathf.Infinity;
        _launched = false;
        GetComponent<Collider2D>().enabled = false;
    }

    string IPersistant.Id
    {
        get => _id;
        set => _id = value;
    }

    public void SaveData()
    {
        if(!persistant || _id.Length < 1 || !ES3.FileExists()) return;
        ES3.Save(_id + "Period", _realCooldownPeriod);
        ES3.Save(_id + "Launching", _launched);
    }
    public void LoadData()
    {
        if(!persistant || _id.Length < 1) return;
        _realCooldownPeriod = ES3.Load(_id + "Period", _realCooldownPeriod);
        _launched = ES3.Load(_id + "Launching", _launched);
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
