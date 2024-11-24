using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamLauncher : MonoBehaviour, IPersistant
{
    [Header("Laser Beam Properties")]
    [SerializeField] private float distanceRay = 100f;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private LayerMask collidableLayer;
    [SerializeField] private float laserBeamRadius;
    [SerializeField] private float launchPeriod;
    [SerializeField] private float cooldownPeriod;
    private bool _launched;
    [SerializeField] bool persistant = true;
    [Header("Player Interaction")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool forceRespawn;
    [Header("Vfx")]
    [SerializeField] private ParticleSystem explodeParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    private float _forceRespawnTimer = 0f;
    private float _launchTimer = 0f;
    LineRenderer _laserBeamRenderer;
    private PlayerController _player;
    [SerializeField] private string _id;
    private float _realCooldownPeriod;
    private Harmable _harmableBehavior;

    void Awake()
    {
        _laserBeamRenderer = GetComponent<LineRenderer>();
        _harmableBehavior= GetComponent<Harmable>();
        _realCooldownPeriod = cooldownPeriod;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _launched = false;
        // Vfx
        explodeParticles.Stop();
        smokeParticles.Stop();
        explodeParticles.Clear();
        smokeParticles.Clear();
    }

    private void Update()
    {
        ShootLaser();
        HandleTimer();
        HandleVfx();
    }

    void HandleVfx()
    {
        if(_harmableBehavior)
        {
            if (_harmableBehavior.HitPointsNormalized <= .5f && !smokeParticles.isPlaying)
            {
                smokeParticles.Play();
            }
        }
    }
    void ShootLaser()
    {
        ShootLaser(_launched, laserBeamRadius);
    }

    void ShootLaser(bool launched, float radius)
    {
        if(!launched)
        {
            _laserBeamRenderer.enabled = false;
            return;
        }
        _laserBeamRenderer.enabled = true;
        var ray = Physics2D.Raycast(laserPoint.position, transform.right, Mathf.Infinity, collidableLayer);
        if (ray)
        {
            Render2DRay(laserPoint.position, ray.point, radius);
        }
        else
        {
            Render2DRay(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay, radius);
        }
        HarmableDetection(Physics2D.CircleCast(laserPoint.position, radius, transform.right, Mathf.Infinity, collidableLayer));
    }

    void HandleTimer()
    {
        _launchTimer += Time.deltaTime;
        if (_launchTimer >= (_launched ? launchPeriod : _realCooldownPeriod))
        {
            _launched = !_launched;
            _laserBeamRenderer.enabled = _launched;
            _launchTimer = 0f;
        }
        else if (!_launched && _realCooldownPeriod - _launchTimer < 1f)
        {
            _laserBeamRenderer.enabled = true;
            var ray = Physics2D.Raycast(laserPoint.position, transform.right, Mathf.Infinity, collidableLayer);
            if (ray)
            {
                Render2DRay(laserPoint.position, ray.point, .001f, 0f);
            }
            else
            {
                Render2DRay(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay, .001f, 0f);
            }
        }
    }
    void HarmableDetection(RaycastHit2D ray)
    {
        if(!ray) return;
        if (ray.collider.CompareTag("Player"))
        {
            _player.playerHarmable.Harm(damage);
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

    public bool Initialized { get; set; }

    public void OnInitialize()
    {
        if(persistant) return;
        LoadData();
    }
    public void OnReset()
    {
        if(persistant) return;
        _launched = true;
        _forceRespawnTimer = 0f;
        _launchTimer = 0f;
        _realCooldownPeriod = cooldownPeriod;
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols) col.enabled = true;
        // Vfx
        explodeParticles.Stop();
        smokeParticles.Stop();
        explodeParticles.Clear();
        smokeParticles.Clear();
    }
    void Render2DRay(Vector2 startPos, Vector2 endPos, float width, float endWidthAddition = .15f)
    {
        _laserBeamRenderer.SetPosition(0, startPos);
        _laserBeamRenderer.SetPosition(1, endPos);
        _laserBeamRenderer.startWidth = width + .1f;
        _laserBeamRenderer.endWidth = width + .1f + endWidthAddition;
    }
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(laserPoint) Gizmos.DrawLine(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay);    
    }
    #endif
    
    public void Die()
    {
        // Stop laser
        _realCooldownPeriod = Mathf.Infinity;
        _launched = false;
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols) col.enabled = false;
        // Vfx
        explodeParticles.Play();
        smokeParticles.Play();
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
