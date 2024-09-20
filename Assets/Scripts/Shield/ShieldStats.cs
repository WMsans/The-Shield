using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Stats", menuName = "Player Controller/Shield Stats", order = 2)]
public class ShieldStats : ScriptableObject
{
    [Header("Layers")] [Tooltip("Set this to the layer shield is collided on")]
    public LayerMask GroundLayer;
    [Tooltip("Set this to the layer shield is targeted on")]
    public LayerMask TargetLayer;
    [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;
    [Tooltip("The force to apply to the player")]
    public float ForceToPlayer = 200;
    [Tooltip("The max number of change in direction")]
    public int MaxChangeDirection = 3;
    [Tooltip("The max number of target distance")]
    public float MaxTargetDistance = 7.5f;
    [Tooltip("The range of bounced angle")]
    public float MaxAngle = 45;
    [Tooltip("The minimum distance to be catched by the player")]
    public float HandRange = 0.5f;
    [Tooltip("The player's cool down time to throw shield")]
    public float CoolDownTime = .15f;
}
