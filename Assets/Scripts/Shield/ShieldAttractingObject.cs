using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldAttractingObject : MonoBehaviour
{
    public Collider2D Col { get; private set; }
    public Vector2 GetPosition()
    {
        return transform.position;
    }
}
