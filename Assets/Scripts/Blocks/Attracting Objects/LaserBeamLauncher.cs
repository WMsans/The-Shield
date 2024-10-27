using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamLauncher : ShieldAttractingObject
{
    [Header("Laser Beam Properties")]
    [SerializeField] private float distanceRay = 100f;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private LayerMask collidableLayer;
    [SerializeField] private float laserBeamRadius;
    [Header("Player Interaction")]
    [SerializeField] private float attractDis = 100f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool forceRespawn;
    private float _forceRespawnTimer = 0f;
    LineRenderer _laserBeamRenderer;
    private PlayerController _player;
    public override float AttractDistance => attractDis;

    private new void Awake()
    {
        base.Awake();
        _laserBeamRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    private void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        var ray = Physics2D.Raycast(laserPoint.position, transform.right, Mathf.Infinity, collidableLayer);
        if (ray)
        {
            Render2DRay(laserPoint.position, ray.point);
        }
        else
        {
            Render2DRay(laserPoint.position, laserPoint.right * distanceRay);
        }
        HarmableDetection(Physics2D.CircleCast(laserPoint.position, laserBeamRadius, transform.right, Mathf.Infinity, collidableLayer));
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
    void Render2DRay(Vector2 startPos, Vector2 endPos)
    {
        _laserBeamRenderer.SetPosition(0, startPos);
        _laserBeamRenderer.SetPosition(1, endPos);
        _laserBeamRenderer.startWidth = laserBeamRadius + .1f;
        _laserBeamRenderer.endWidth = laserBeamRadius + .25f;
    }
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(laserPoint) Gizmos.DrawLine(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay);    
    }
    #endif
}
