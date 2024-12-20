using UnityEngine;

[CreateAssetMenu(fileName = "New Player Stats", menuName = "Custom Assets/Player Controller/Player Stats", order = 1)]
public class PlayerStats : ScriptableObject
{
    [Header("LAYERS")] [Tooltip("Set this to the layer your player is collided on")]
    public LayerMask GroundLayer;
    
    [Tooltip("Set this to the layer your player is disabled on in respawn state")]
    public LayerMask RespawnLayer;

    [Header("INPUT")] [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
    public bool SnapInput = true;

    [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;

    [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;
    
    [Tooltip("The top horizontal movement speed when bounced")]
    public float MaxBouncedSpeed = 20;
    
    [Tooltip("The time of bounced in seconds")]
    public float BounceTime = 0.5f;

    [Tooltip("The player's capacity to gain horizontal speed")]
    public float Acceleration = 120;

    [Tooltip("The pace at which the player comes to a stop")]
    public float GroundDeceleration = 60;

    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float AirDeceleration = 30;

    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;
    
    [Tooltip("The force applied when player is pushed against wall")]
    public float WallingForce = 1.5f;
    
    [Tooltip("The force applied when player does a neutral jump against wall")]
    public float WallingNeutralForce = 1.5f;
    
    [Tooltip("The detection distance for wall detection")]
    public float WallerDistance = 0.05f;
    
    [Tooltip("The push acceleration sustain time in seconds")]
    public float PushAccelerationSustainTime = 1f;
    
    [Tooltip("Horizontal acceleration ratio after pushed by shield")][Range(0,1)]
    public float PushAcceleration = .3f;
    
    [Tooltip("The buff duration for player to go over the walls in seconds")]
    public float BouncedBuffTime = 0.5f;

    [Header("JUMP")] [Tooltip("The immediate velocity applied when jumping")]
    public float JumpPower = 36;
    
    [Tooltip("The immediate velocity applied when climbing up the ledge")]
    public float ClimbPower = 12f;

    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;

    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;

    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public float CoyoteTime = .15f;

    [Tooltip("The amount of time we buffer a jump in seconds. This allows jump input before actually hitting the ground")]
    public float JumpBuffer = .2f;
    
    [Tooltip("The amount of time we buffer a grab down")]
    public float LedgeBuffer = .2f;
    
    [Header("Others")] [Tooltip("The time of invincible time after getting hit")]
    public float InvincibilityTime = 2f;
    
    public float PlayerMeleeKnockback = 500f;
}
