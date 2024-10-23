using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldAttractingObject : MonoBehaviour
{
    public Collider2D Col {get; private set;}
    public float AttractDistance {get; protected set;}
    protected void Awake()
    {
        Col = GetComponent<Collider2D>();
        AttractDistance = Mathf.Infinity;
    }
}
