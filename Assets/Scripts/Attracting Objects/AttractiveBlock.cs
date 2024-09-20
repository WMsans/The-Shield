using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractiveBlock : ShieldAttractingObject
{
    private void Awake()
    {
        Col = GetComponent<Collider2D>();
    }
}
