using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnchorPoint : MonoBehaviour
{
    [SerializeField] float velocityStayTime;
    public Vector2 AnchorPointVelocity => _velocityStayed && _velocityStayTimer > 0 ? _stayedVelocity : _nowVelocity;
    public Transform Target { get; private set; }
    private Vector2 _initialTargetPos;
    private Vector2 TargetPos => Target == null ? transform.position : (Vector2)Target.position - _initialTargetPos;
    private bool _velocityStayed;
    private float _velocityStayTimer = 0f;
    private Vector2 _stayedVelocity;
    private Vector2 _nowVelocity;
    void Update()
    {
        HandleTimer();
    }
    void HandleTimer()
    {
        _velocityStayTimer = Mathf.Max(0f, _velocityStayTimer - Time.deltaTime);
    }
    private void FixedUpdate()
    {
        HandleVelocity();
    }
    
    void HandleVelocity()
    {
        var newVel = TargetPos - (Vector2)transform.position;
        
        if (newVel.magnitude < _nowVelocity.magnitude && !_velocityStayed)
        {
            // Start the velocity timer
            StartVelocityStayTimer();
        }
        else if (newVel.magnitude > _nowVelocity.magnitude)
        {
            ResetVelocityStayTimer();
        }
        
        _nowVelocity = newVel;  
        transform.position = TargetPos;
    }

    void StartVelocityStayTimer()
    {
        _stayedVelocity = _nowVelocity;
        _velocityStayTimer = velocityStayTime;
        _velocityStayed = true;
    }

    void ResetVelocityStayTimer()
    {
        _stayedVelocity = _nowVelocity;
        _velocityStayTimer = 0f;
        _velocityStayed = false;
    }
    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            Target = null;
            return;
        }
        Target = target;
        _initialTargetPos = Target.position - transform.position;
    }
}
