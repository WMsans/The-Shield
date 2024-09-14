using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Stats", menuName = "Player Controller/Shield Stats", order = 2)]
public class ShieldStats : ScriptableObject
{
    [Header("Layers")] [Tooltip("Set this to the layer shield is collided on")]
    public LayerMask GroundLayer;
    [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;
    [Tooltip("The range of bounced angle")]
    public float MaxAngle = 45;
    [Tooltip("The minimum distance to be catched by the player")]
    public float HandRange = 0.5f;
}
