using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamLauncher : ShieldAttractingObject
{
    [SerializeField] private float distanceRay = 100f;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float attractDis = 100f;
    LineRenderer _laserBeamRenderer;

    public override float AttractDistance => attractDis;

    private new void Awake()
    {
        base.Awake();
        _laserBeamRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        var ray = Physics2D.Raycast(laserPoint.position, transform.right, Mathf.Infinity, groundLayer);
        if (ray)
        {
            Render2DRay(laserPoint.position, ray.point);
        }
        else
        {
            Render2DRay(laserPoint.position, laserPoint.right * distanceRay);
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
