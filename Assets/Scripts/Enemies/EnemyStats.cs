using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Custom Assets/Enemy/Enemy Stats", order = 1)]
public class EnemyStats : ScriptableObject
{
    [Header("Layers")] 
    public LayerMask whatIsGround;
    
    [Header("Behaviour")]
    public Enums.EnemyStates startingState;
    
    [Header("Movement")]
    public float maxSpeed;
    
    public float acceleration;
    
    public float deceleration;
    
    public float airDeceleration = 30;
    
    [Header("Player Detection")]
    public float playerDistance;
    
    public bool collidingWithPlayer = false;

    public bool damageCollidingWithPlayer = true;
}
