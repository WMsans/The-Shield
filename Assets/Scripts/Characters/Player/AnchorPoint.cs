using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnchorPoint : MonoBehaviour
{
    [SerializeField] float velocityStayTime;
    public Vector2 AnchorPointVelocity => _stayedVelocity * 15f;
    public Transform Target { get; private set; }
    private Vector2 _initialTargetPos;
    public Vector2 TargetPos => Target == null ? transform.position : (Vector2)Target.position - _initialTargetPos;
    private bool _velocityStayed;
    private float _velocityStayTimer = 0f;
    private Vector2 _stayedVelocity;
    private Vector2 _nowVelocity;
    private Vector2 _lastPos;
    void Update()
    {
        transform.position = TargetPos;
        HandleTimer();
    }
    void HandleTimer()
    {
        _velocityStayTimer -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        HandleVelocity();
        Debug.Log(_velocityStayTimer);
    }
    
    void HandleVelocity()
    {
        var newVel = TargetPos - _lastPos;
        
        if (newVel.sqrMagnitude < _stayedVelocity.sqrMagnitude && !_velocityStayed)
        {
            // Start the velocity timer
            StartVelocityStayTimer();
        }
        else if (newVel.sqrMagnitude >= _stayedVelocity.sqrMagnitude)
        {
            ResetVelocityStayTimer();
            _stayedVelocity = newVel;
        }
        else if(_velocityStayTimer <= 0f)_stayedVelocity = newVel;
        
        _nowVelocity = newVel; 
        _lastPos = transform.position;
    }

    void StartVelocityStayTimer()
    {
        _velocityStayTimer = velocityStayTime;
        _velocityStayed = true;
    }

    void ResetVelocityStayTimer()
    {
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
