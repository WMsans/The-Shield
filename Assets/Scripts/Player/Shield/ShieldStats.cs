using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Stats", menuName = "Custom Assets/Player Controller/Shield Stats", order = 2)]
public class ShieldStats : ScriptableObject
{
    [Header("Layers")] [Tooltip("Set this to the layer shield is collided on")]
    public LayerMask GroundLayer;
    [Tooltip("Set this to the layer shield is targeted on")]
    public LayerMask TargetLayer;
    [Tooltip("Set this to the layer player is at")]
    public LayerMask PlayerLayer;
    [Header("MOVEMENT")] [Tooltip("The top movement speed")]
    public float MaxSpeed = 14;
    [Tooltip("The horizontal force to apply to the player")]
    public float HorizontalForceToPlayer = 15;
    [Tooltip("The vertical force to apply to the player")]
    public float VarticleForceToPlayer = 15;
    [Tooltip("The horizontal force to apply to the player when opposing")]
    public float HorizontalOpposeForceToPlayer = 20;
    [Tooltip("The max number of change in direction")]
    public int MaxChangeDirection = 3;
    [Tooltip("The max target distance")]
    public float MaxTargetDistance = 7.5f;
    [Tooltip("The min target distance")]
    public float MinTargetDistance = 3f;
    [Tooltip("The minimum distance to be caught by the player")]
    public float HandRange = 0.5f;
    [Tooltip("The player's cool down time to throw shield")]
    public float CoolDownTime = .15f;
    [Header("Detection")][Tooltip("The detection radius for collision")]
    public float DetectionRadius = 0.5f;
    [Tooltip("The detection ray length for enemy")]
    public float DetectionRayLength = 1f;
    [Tooltip("The pre-input for attack in seconds")]
    public float PreInputTime = 0.25f;
}
