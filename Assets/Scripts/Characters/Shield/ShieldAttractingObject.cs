using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldAttractingObject : MonoBehaviour, IResetable
{
    public Collider2D Col {get; protected set;}
    public virtual float AttractDistance => Mathf.Infinity;
    private Vector3 _oriPosition;
    private bool _initialized;

    void OnEnable()
    {
        OnInitialize();
    }
    protected void Awake()
    {
        Col = GetComponent<Collider2D>();
    }

    bool IResetable.Initialized
    {
        get => _initialized;
        set => _initialized = value;
    }

    public virtual void OnInitialize()
    {
        _oriPosition = transform.position;
        _initialized = true;
    }

    public virtual void OnReset()
    {
        if (!_initialized) return;
        transform.position = _oriPosition;
    }
}
