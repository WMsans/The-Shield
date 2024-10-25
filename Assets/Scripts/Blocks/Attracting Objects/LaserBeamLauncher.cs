using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamLauncher : ShieldAttractingObject
{
    [SerializeField] private float distanceRay = 100f;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private LayerMask collidableLayer;
    [SerializeField] private float attractDis = 100f;
    [SerializeField] private float damage = 1f;
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
            PlayerDetection(ray);
            Render2DRay(laserPoint.position, ray.point);
        }
        else
        {
            Render2DRay(laserPoint.position, laserPoint.right * distanceRay);
        }
    }

    void PlayerDetection(RaycastHit2D ray)
    {
        if (ray.collider.CompareTag("Player"))
        {
            // Player detected, harm player
            _player.Harm(damage);
        }
    }
    void Render2DRay(Vector2 startPos, Vector2 endPos)
    {
        _laserBeamRenderer.SetPosition(0, startPos);
        _laserBeamRenderer.SetPosition(1, endPos);
    }
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(laserPoint != null) Gizmos.DrawLine(laserPoint.position, laserPoint.position + laserPoint.right * distanceRay);    
    }
    #endif
}
