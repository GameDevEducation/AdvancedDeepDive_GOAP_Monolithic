using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[CreateAssetMenu(fileName = "Character Motor Config", menuName = "Injaia/Character/Motor Config", order = 1)]
public class CharacterMotorConfig : ScriptableObject
{
    public enum ERunMode
    {
        Toggle,
        Hold
    }

    [Header("Character")]
    public float CharacterHeight = 1.8f;
    public float CharacterRadius = 0.4f;

    [Header("General Movement")]
    public float WalkSpeed = 10f;
    public float GroundedThreshold = 0.1f;
    public float CollisionBuffer = 0.1f;
    [Range(0f, 1f)] public float MovementBlendRate = 1f;
    public PhysicMaterial DefaultMaterial;
    public PhysicMaterial MaterialWhenJumping;

    [Header("Movement Constraints")]
    public float StepCheckLookAhead = 0.45f;
    public float MaxStepUpDistance = 0.3f;
    public float MinStepDownDistance = 0.3f;
    public float MaxSlope = 45f;

    public LayerMask WalkableMask = ~0;

    [Header("Running")]
    public bool CanRun = true;
    [ConditionalField(nameof(CanRun))] public ERunMode RunMode = ERunMode.Hold;
    [ConditionalField(nameof(CanRun))] public float RunSpeed = 15f;

    [Header("Jumping")]
    public bool CanJump = true;
    [ConditionalField(nameof(CanJump))] public bool CanDoubleJump = true;
    [ConditionalField(nameof(CanJump))] public float JumpHeight = 1f;
    [ConditionalField(nameof(CanJump))] public float JumpVelocity = 4f;
    [ConditionalField(nameof(CanJump))] public float JumpVelocityBlend = 0.75f;
    [ConditionalField(nameof(CanJump))] public float JumpCeilingCheck = 0.1f;
    [ConditionalField(nameof(CanJump))] public LayerMask CeilingMask = ~0;
    public float FallForce = 0.5f;

    [Header("In Air")]
    public bool AirControl = false;
    [ConditionalField(nameof(AirControl))] public float InAirSpeed = 3f;

    [Header("Camera")]
    public bool RestrictVerticalRotation = true;
    public float VerticalRotation_Min = -85f;
    public float VerticalRotation_Max = 85f;

    [Header("Audio")]
    public float FootstepInterval = 0.5f;
    public AnimationCurve FootstepFrequencyWithSpeed;
}
