using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class ControlSchemeChanged : UnityEvent<PlayerInput> {}

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour, IPausable
{
    #pragma warning disable 0649
    [Header("Configuration")]
    [SerializeField] CharacterMotorConfig Config;

    [Header("Events")]
    [SerializeField] UnityEvent OnPlayFootstepAudio;
    [SerializeField] UnityEvent OnPlayJumpAudio;
    [SerializeField] UnityEvent OnPlayDoubleJumpAudio;
    [SerializeField] UnityEvent OnPlayLandAudio;

    [SerializeField] UnityEvent OnToggleSettingsMenu;

    [SerializeField] ControlSchemeChanged OnControlSchemeChanged;

    [Header("Debugging")]
    [SerializeField] bool DEBUG_ShowStepRays = false;
    #pragma warning restore 0649

    protected bool IsRunning = false;
    protected bool IsGrounded = true;
    protected bool IsJumping = false;

    protected int JumpCount = 0;
    protected float JumpTargetY = float.MinValue;
    protected float OriginalDrag = 0f;

    protected bool LockCursor = true;
    protected bool EnableUpdates = true;

    protected float TimeUntilNextFootstep = -1f;

    protected Rigidbody CharacterRB;
    protected Collider CharacterCollider;
    protected CinemachineVirtualCamera Camera;
    protected float CameraPitch = 0f;

    protected bool IsCameraLocked
    {
        get
        {
            return !Cursor.visible && Cursor.lockState == CursorLockMode.Locked;
        }
    }

    void Awake()
    {
    }

    protected virtual void Start()
    {
        PauseManager.Instance.RegisterPausable(this);
        
        CharacterRB = GetComponent<Rigidbody>();
        CharacterCollider = GetComponent<Collider>();
        Camera = GetComponentInChildren<CinemachineVirtualCamera>();

        OriginalDrag = CharacterRB.drag;
        CharacterCollider.material = Config.DefaultMaterial;

        UpdateCursorLock(LockCursor);
    }

    #region Input Manager
    protected Vector2 _Internal_MovementInput;
    public void OnMove(InputValue value)
    {
        _Internal_MovementInput = value.Get<Vector2>();
    }    

    protected Vector2 _Internal_LookInput;
    public void OnLook(InputValue value)
    {
        _Internal_LookInput = value.Get<Vector2>();
    }  

    protected bool _Internal_JumpInput;
    public void OnJump(InputValue value)
    {
        _Internal_JumpInput = value.Get<float>() > 0.5f;
    }

    protected bool _Internal_SprintInput;
    public void OnSprint(InputValue value)
    {
        _Internal_SprintInput = value.Get<float>() > 0.5f;
    }

    public void OnSettings(InputValue value)
    {
        if (EnableUpdates)
            OnToggleSettingsMenu?.Invoke();
    }

    public void OnControlsChanged(PlayerInput input)
    {
        OnControlSchemeChanged?.Invoke(input);
    }
    #endregion

    #region Camera Handling
    protected virtual Vector2 GetCameraInput()
    {        
        return new Vector2(_Internal_LookInput.x * SettingsManager.Settings.Camera.Sensitivity_X, 
                           _Internal_LookInput.y * SettingsManager.Settings.Camera.Sensitivity_Y * (SettingsManager.Settings.Camera.Invert_YAxis ? 1f : -1f));
    }

    protected virtual void Update()
    {
        // do nothing if updating is turned off or if paused
        if (!EnableUpdates || PauseManager.IsPaused)
            return;

        // retrieve the camera input (already has sensitivity and inversion applied)
        Vector2 cameraInput = GetCameraInput() * Time.deltaTime;

        // determine the new rotations
        Quaternion newCharacterRotation = transform.localRotation * Quaternion.Euler(0f, cameraInput.x, 0f);
        CameraPitch = Mathf.Clamp(CameraPitch + cameraInput.y, Config.VerticalRotation_Min, Config.VerticalRotation_Max);
        Quaternion newCameraRotation = Quaternion.Euler(CameraPitch, 0f, 0f);

        // update the rotations
        transform.localRotation = newCharacterRotation;
        Camera.transform.localRotation = newCameraRotation;
    }
    #endregion

    #region Movement Handling
    protected virtual void UpdateRunning()
    {
        if (!IsGrounded)
        {
            IsRunning = false;
            return;
        }

        if (Config.RunMode == CharacterMotorConfig.ERunMode.Hold)
            IsRunning = _Internal_SprintInput;
        else if (Config.RunMode == CharacterMotorConfig.ERunMode.Toggle)
        {
            if (!IsRunning && _Internal_SprintInput)
                IsRunning = true;
        }
    }

    protected virtual RaycastHit UpdateIsGrounded()
    {
        // raycast to check where the ground is
        RaycastHit hitResult;     
        float groundCheckDistance = Config.GroundedThreshold + (Config.CharacterHeight * 0.5f) - Config.CharacterRadius;
        float workingRadius = Config.CharacterRadius * (1f - Config.CollisionBuffer);
        if (Physics.SphereCast(transform.position, workingRadius, Vector3.down, out hitResult, groundCheckDistance, Config.WalkableMask, QueryTriggerInteraction.Ignore))
        {
            // check if the character is grounded
            IsGrounded = true;
        }
        else
            IsGrounded = false;

        return hitResult;
    }

    protected virtual void FixedUpdate()
    {
        // do nothing if updating is turned off or if paused
        if (!EnableUpdates || PauseManager.IsPaused)
            return;

        // Update if grounded
        bool wasGrounded = IsGrounded;
        RaycastHit hitResult = UpdateIsGrounded();

        // Update if running
        UpdateRunning();

        // read the movement input
        Vector2 movementInput = GetMovementInput();

        // calculat the potential movement vector (handles grounded, not grounded and air control on or off)
        Vector3 movementVector = transform.forward * movementInput.y * CurrentSpeed + 
                                 transform.right   * movementInput.x * CurrentSpeed;
        float originalMagnitude = movementVector.magnitude;

        // if we're grounded then determine how far we move
        if (IsGrounded)
        {
            // project the movement along the plane
            movementVector = Vector3.ProjectOnPlane(movementVector, hitResult.normal);
            movementVector = movementVector.normalized * originalMagnitude;

            // check if moving up and amount is above the slope limit
            if (movementVector.y > float.Epsilon && Vector3.Angle(Vector3.up, hitResult.normal) >= Config.MaxSlope)
            {
                movementVector = Vector3.zero;
            }
        }

        // has a jump been requested?
        bool jumpRequested = Config.CanJump && _Internal_JumpInput;

        // jump can only happen if either we're grounded or we're on the first jump of a double jump
        jumpRequested &= IsGrounded || (Config.CanDoubleJump && (JumpCount < 2));

        // are we at rest?
        if (!jumpRequested && !IsJumping && IsGrounded && CharacterRB.velocity.sqrMagnitude < 0.1f && movementInput.sqrMagnitude < 0.1f)
        {
            CharacterRB.velocity = Vector3.zero;
            CharacterRB.Sleep();
            IsRunning = false;
        }
        else
        {
            // update the character's velocity if on ground or can air control
            if (IsGrounded)
                CharacterRB.velocity = Vector3.Lerp(CharacterRB.velocity, movementVector, Config.MovementBlendRate);
            else if (Config.AirControl)
            {
                movementVector.y = CharacterRB.velocity.y;
                CharacterRB.velocity = Vector3.Lerp(CharacterRB.velocity, movementVector, Config.MovementBlendRate);
            }

            // was a jump requested
            if (jumpRequested)
            {
                // handle jumping audio
                if (JumpCount == 0)
                    OnPlayJumpAudio?.Invoke();
                else
                    OnPlayDoubleJumpAudio?.Invoke();

                ++JumpCount;
                IsJumping = true;
                CharacterRB.drag = 0;
                CharacterCollider.material = Config.MaterialWhenJumping;
                JumpTargetY = transform.position.y + Config.JumpHeight;

                // make sure the jump input is cleared (in case of being held)
                _Internal_JumpInput = false;
            }

            // are we jumping?
            if (IsJumping)
            {
                Vector3 velocity = CharacterRB.velocity;
                velocity.y = Mathf.Lerp(Mathf.Max(0f, velocity.y), Config.JumpVelocity, Config.JumpVelocityBlend);

                CharacterRB.velocity = velocity;

                // have we reached the maximum height?
                float ceilingHitCheckRange = Config.JumpCeilingCheck + Config.CharacterHeight * 0.5f;
                if (transform.position.y >= JumpTargetY ||
                    Physics.Raycast(transform.position, Vector3.up, ceilingHitCheckRange, Config.CeilingMask, QueryTriggerInteraction.Ignore))
                {
                    // if we hit the ceiling then zero out the velocity
                    velocity.y = 0;
                    CharacterRB.velocity = velocity;

                    IsJumping = false;
                }
            }
            else if (!IsGrounded)
            {
                CharacterRB.AddForce(Vector3.down * Config.FallForce, ForceMode.Impulse);
            }
            else
            {
                // if we weren't grounded previously then play the land audio and reset the footstep interval
                if (!wasGrounded)
                {
                    OnPlayLandAudio?.Invoke();
                    HearingManager.Instance.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EJump, 2f);
                    TimeUntilNextFootstep = Config.FootstepInterval;
                }

                // update the footstep audio
                UpdateFootstepAudio();

                // run the look ahead from the feet
                Vector3 stepCheckStart = transform.position - Vector3.up * (Config.CharacterHeight * 0.5f) + (Vector3.up * 0.05f);
                Vector3 stepCheckDirection = movementVector.normalized;

                #if UNITY_EDITOR
                Vector3 DBG_StepStage1StartLoc = stepCheckStart;
                bool DBG_StepStage1Hit = false;
                bool DBG_StepStage2Hit = false;
                #endif // UNITY_EDITOR

                // first step is to check that there is a blocker
                if (Physics.Raycast(stepCheckStart, stepCheckDirection, Config.StepCheckLookAhead, Config.WalkableMask, QueryTriggerInteraction.Ignore))
                {
                    stepCheckStart += Vector3.up * Config.MaxStepUpDistance;

                    #if UNITY_EDITOR
                    DBG_StepStage1Hit = true;
                    #endif // UNITY_EDITOR

                    // now we check if the blocker is clear at the step height
                    if (!Physics.Raycast(stepCheckStart, stepCheckDirection, Config.StepCheckLookAhead, Config.WalkableMask, QueryTriggerInteraction.Ignore))
                    {                     
                        Vector3 candidatePoint = stepCheckStart + (stepCheckDirection * Config.StepCheckLookAhead);

                        // make sure we hit the ground
                        RaycastHit stepCheckHitResult;
                        if (Physics.Raycast(candidatePoint, Vector3.down, out stepCheckHitResult, Config.CharacterHeight * 0.5f, Config.WalkableMask, QueryTriggerInteraction.Ignore))
                        {
                            // trying to step to a valid spot?
                            if (Vector3.Angle(Vector3.up, stepCheckHitResult.normal) < Config.MaxSlope)
                            {
                                transform.position = stepCheckHitResult.point + Vector3.up * Config.CharacterHeight * 0.5f;
                            }
                            #if UNITY_EDITOR
                            else
                                DBG_StepStage2Hit = true;
                            #endif // UNITY_EDITOR  
                        }
                        #if UNITY_EDITOR
                        else
                            DBG_StepStage2Hit = true;
                        #endif // UNITY_EDITOR   
                    }
                    #if UNITY_EDITOR
                    else
                        DBG_StepStage2Hit = true;
                    #endif // UNITY_EDITOR                   
                }

                #if UNITY_EDITOR
                if (DEBUG_ShowStepRays)
                {
                    if (DBG_StepStage1Hit)
                        Debug.DrawLine(stepCheckStart, stepCheckStart + stepCheckDirection * Config.StepCheckLookAhead, DBG_StepStage2Hit ? Color.red : Color.green, 0f);
                    else
                        Debug.DrawLine(DBG_StepStage1StartLoc, DBG_StepStage1StartLoc + stepCheckDirection * Config.StepCheckLookAhead, DBG_StepStage1Hit ? Color.blue : Color.yellow, 0f);
                }
                #endif // UNITY_EDITOR

                // back on the ground - reset drag, physics material and jump count
                CharacterRB.drag = OriginalDrag;
                CharacterCollider.material = Config.DefaultMaterial;
                JumpCount = 0;
                IsJumping = false;
            }
        }
    }

    public float CurrentSpeed
    {
        get
        {
            return IsGrounded ? (IsRunning ? Config.RunSpeed : Config.WalkSpeed) : 
                                (Config.AirControl ? Config.InAirSpeed : 0);
        }
    }

    protected virtual Vector2 GetMovementInput()
    {
        return _Internal_MovementInput;
    }

    protected void UpdateFootstepAudio()
    {
        // update the time until the next footstep
        if (TimeUntilNextFootstep > 0)
        {
            float footstepTimeScale = 1f + Config.FootstepFrequencyWithSpeed.Evaluate(CharacterRB.velocity.magnitude / Config.RunSpeed);

            TimeUntilNextFootstep -= Time.deltaTime * footstepTimeScale;
        }

        // time to play the sound?
        if (TimeUntilNextFootstep <= 0)
        {
            OnPlayFootstepAudio?.Invoke();
            HearingManager.Instance.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EFootstep, IsRunning ? 2f : 1f);

            TimeUntilNextFootstep = Config.FootstepInterval;

            return;
        }
    }
    #endregion

    #region Cursor Handling
    public void UpdateCursorLock(bool newValue)
    {
        LockCursor = newValue;

        // update the cursor state
        Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !LockCursor;
    }
    #endregion

    #region Update Toggling
    public void SetCanUpdate(bool newValue)
    {
        EnableUpdates = newValue;
    }
    #endregion

    #region IPausable
    public bool OnPauseRequested()  { return true; }
    public bool OnResumeRequested() { return true; }

    public void OnPause() { }
    public void OnResume() { }
    #endregion    
}
