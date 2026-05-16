using UnityEngine;
using UnityEngine.InputSystem;

public class NovoMovimento : MonoBehaviour
{
    public Animator SpritePlayer;
    public float moveSpeed;
    public Transform groundCheck;
    public float ImpulseTimer;
    public float ImpulseForce;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private AudioSource somQueda;
    private BoxCollider2D groundCheckCollider;
    private bool isGrounded;
    private bool isChargingJump = false;
    private float initialImpulseTimer;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset; // assign your InputSystem_Actions asset here

    // runtime action refs
    private InputAction moveAction;
    private InputAction jumpAction;
    private float currentMoveInput = 0f;
    private bool isMovementEnabled = true;
    private bool isStartupDelayActive = true; // Prevents movement during initial 2 second delay
    
    // Dash/Air Boost System
    private bool isChargingDash = false;
    private float dashChargeTimer = 0f;
    private float dashChargeDirection = 0f; // Store direction when dash starts
    private bool isDashBoostActive = false; // Prevent basic movement from interfering
    private float lastDashTime = -999f; // Track when last dash was released
    private int dashesRemaining = 1; // Track dashes left in current air state
    private float airStateStartTime = 0f; // When player entered air state
    private bool wasGroundedLastFrame = true; // Detect ledge fall
    // Animation state cache
    private enum AnimationState { Idle, Left, Right, Hold, Jump }
    private AnimationState currentAnimationState;
    private bool hasAnimationState;
    private bool holdPlayedThisCharge = false;
    private Collider2D playerCollider;
    private bool isInGunk = false;
    private bool hasGunkJumpReset = false;
    private float nextGunkJumpResetTime = 0f;
    private float baseGravityScale = 1f;
    private float baseLinearDamping = 0f;
    private float landingMomentumX = 0f;
    private float landingMomentumDecayTimer = 0f;
    private bool wasInGunkLastFrame = false;

    [Header("Dash Settings")]
    public int maxAirDashes = 1; // Max dashes per jump
    public float dashChargeMaxTime = 1f;
    public float dashBoostForce = 15f;
    public float dashFallSpeed = 0.5f; // slow descent while charging
    public float ledgeFallCooldown = 0.2f; // Delay before dash is usable when falling off ledge

    [Header("Gunk")]
    public LayerMask gunkLayer;
    public float gunkSlowFactor = 0.5f; // Multiplier to apply to moveSpeed when in gunk
    public float gunkGravityFactor = 0.6f; // Multiplier to apply to gravity while in gunk
    public float gunkLinearDamping = 8f; // Extra drag while in gunk to slow all movement
    public float gunkJumpResetInterval = 0.5f; // While in gunk, refresh jump access every interval

    [Header("Landing Momentum")]
    public float landingMomentumRetentionFactor = 0.4f; // How much dash X velocity to keep on landing (0-1)
    public float landingMomentumDecayTime = 0.3f; // Seconds to gradually decay momentum

    [Header("Slope / Slide Settings")]
    [Tooltip("Distance used to raycast downwards to detect ground normal for slope calculations.")]
    public float slopeCheckDistance = 0.6f;
    [Tooltip("How fast the player passively slides down slopes when not actively climbing.")]
    public float slideSpeed = 3f;
    [Tooltip("Minimum angle (degrees) considered a slope to start sliding.")]
    public float slopeSlideAngleThreshold = 5f;
    [Tooltip("Maximum slope angle for normalization (used to scale sliding).")]
    public float maxSlopeAngle = 60f;
    [Tooltip("How much dash boost contributes to slope-climb ability (multiplier).")]
    public float dashClimbStrengthFactor = 1f;
    [Tooltip("Time window after releasing a dash during which slope-climb is allowed.")]
    public float dashClimbWindow = 0.3f;

    // runtime slope/dash tracking
    private float lastDashBoostMagnitude = 0f;
    private float lastDashReleaseTime = -999f;

    [Header("Queda")]
    [SerializeField] private float fallMultiplier = 2f; // descida: quanto mais rápido cai
    [SerializeField] private float lowJumpMultiplier = 2f; // se soltar salto cedo, desce mais rápido

    [Header("Animações")]
    [SerializeField] private string idleAnimation = "idle";
    [SerializeField] private string leftAnimation = "left";
    [SerializeField] private string rightAnimation = "right";
    [SerializeField] private string holdAnimation = "hold";
    [SerializeField] private string jumpAnimation = "jump";
    [SerializeField] private string bigFallAnimation = "BigFall";
    [SerializeField] private string knockedAnimation = "knocked";
    [SerializeField] private string gettingUpAnimation = "GettingUp";
    [Header("Big Fall")]
    [Tooltip("Vertical velocity threshold (absolute) used to detect a big fall on landing.")]
    public float bigFallVelocityThreshold = 10f;
    [SerializeField] private float moveDeadZone = 0.1f;

    // Special animation state control
    private bool isInSpecialAnimation = false; // when true, normal movement/animations are locked
    private bool isInKnocked = false;
    private bool knockedPlayedOnce = false;
    private float prevYVelocity = 0f;

    private void OnEnable()
    {
        if (inputActionsAsset != null)
        {
            var map = inputActionsAsset.FindActionMap("Player", true);
            moveAction = map.FindAction("Move", true);
            jumpAction = map.FindAction("Jump", true);

            if (moveAction != null) moveAction.Enable();
            if (jumpAction != null)
            {
                jumpAction.Enable();
                jumpAction.started += OnJumpStarted;
                jumpAction.canceled += OnJumpCanceled;
            }
        }
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            baseGravityScale = rb.gravityScale;
            baseLinearDamping = rb.linearDamping;
        }

        playerCollider = this.GetComponent<Collider2D>();
        groundCheckCollider = groundCheck.GetComponent<BoxCollider2D>();
        somQueda = this.GetComponent<AudioSource>();
        
        if (groundCheckCollider == null)
            Debug.LogError("NovoMovimento: groundCheck child does not have a BoxCollider2D! Ground detection will not work.");

        if (playerCollider == null)
            Debug.LogError("NovoMovimento: player does not have a Collider2D! Gunk detection will not work.");
        
        initialImpulseTimer = ImpulseTimer;
        
        // Register animator with TimeMaster for time scale syncing
        if (TImeMaster.Instance != null)
        {
            TImeMaster.Instance.RegisterAnimator(SpritePlayer);
        }

        // At game start: play knocked animation as the initial state
        if (SpritePlayer != null)
        {
            SpritePlayer.Play(knockedAnimation);
            isInSpecialAnimation = true;
            isInKnocked = true;
            // mark as not yet played once; we will set it after one full cycle
            knockedPlayedOnce = false;
            DisableMovement();
            StartCoroutine(MarkKnockedPlayedOnce());
        }

        // Disable movement for the first 2 seconds after game start
        StartCoroutine(EnableMovementAfterDelay(2f));
    }

    void Update()
    {
        // (Animations updated later after reading grounded state & inputs)

        #region Basic Movement
        // Check if grounded using the BoxCollider2D from groundCheck child
        if (groundCheckCollider != null)
        {
            isGrounded = groundCheckCollider.IsTouchingLayers(groundLayer);
        }
        else
        {
            isGrounded = false;
            if (groundCheck != null)
                Debug.LogWarning("groundCheckCollider not found! Make sure groundCheck child has a BoxCollider2D component.");
        }

        if (playerCollider != null)
        {
            isInGunk = playerCollider.IsTouchingLayers(gunkLayer);
        }
        else
        {
            isInGunk = false;
        }

        if (isInGunk)
        {
            if (!wasInGunkLastFrame)
                print("In gunk");
            if (Time.time >= nextGunkJumpResetTime)
            {
                hasGunkJumpReset = true;
                // Also reset available dashes when granting a jump reset from gunk
                dashesRemaining = maxAirDashes;
                nextGunkJumpResetTime = Time.time + gunkJumpResetInterval;
            }
        }
        else
        {
            if (wasInGunkLastFrame)
                print("Out of gunk");
            hasGunkJumpReset = false;
            nextGunkJumpResetTime = 0f;
        }
        wasInGunkLastFrame = isInGunk;

        if (rb != null)
        {
            float gravityFactor = isInGunk ? Mathf.Max(0f, gunkGravityFactor) : 1f;
            rb.gravityScale = baseGravityScale * gravityFactor;
            rb.linearDamping = isInGunk ? Mathf.Max(baseLinearDamping, gunkLinearDamping) : baseLinearDamping;
        }
        
        // Reset dashes when grounded
        if (isGrounded && !wasGroundedLastFrame)
        {
            dashesRemaining = maxAirDashes;
            // Preserve some momentum from dash/air movement when landing
            landingMomentumX = rb.linearVelocity.x * landingMomentumRetentionFactor;
            landingMomentumDecayTimer = landingMomentumDecayTime;

            // Detect heavy fall on landing
            if (prevYVelocity < -bigFallVelocityThreshold)
            {
                TriggerBigFallSequence();
            }
        }
        
        // Decay landing momentum over time
        if (landingMomentumDecayTimer > 0f)
        {
            landingMomentumDecayTimer -= Time.deltaTime;
            if (landingMomentumDecayTimer <= 0f)
            {
                landingMomentumX = 0f;
            }
        }
        
        // Detect ledge fall (entering air without jumping)
        if (!isGrounded && wasGroundedLastFrame && !isChargingJump)
        {
            airStateStartTime = Time.time; // Start cooldown for dash on ledge fall
        }
        
        wasGroundedLastFrame = isGrounded;
        
        // Clear boost protection when grounded
        if (isDashBoostActive && isGrounded)
        {
            isDashBoostActive = false;
            // Preserve momentum from the dash when boost ends on landing
            landingMomentumX = rb.linearVelocity.x * landingMomentumRetentionFactor;
            landingMomentumDecayTimer = landingMomentumDecayTime;
        }
        
        // Read movement input
        if (moveAction != null && moveAction.enabled)
        {
            currentMoveInput = moveAction.ReadValue<Vector2>().x;
        }

        // Update animations now that we have grounded state and input
        // Skip normal animation updates while a special animation (BigFall/knocked/GettingUp) is active
        if (!isInSpecialAnimation)
        {
            UpdateAnimation(currentMoveInput);
        }

        // Ground: Enable movement
        if (isGrounded)
        {
            if (!isChargingJump && !isStartupDelayActive)
            {
                ImpulseTimer = initialImpulseTimer;
                EnableMovement();
            }
        }
        // Mid-air: Disable normal movement (reserve for dash boost), but protect boost velocity
        else
        {
            if (!isDashBoostActive)
            {
                DisableMovement();
            }
        }

        // Apply horizontal movement when enabled (skip entirely if boost is active)
        if (!isDashBoostActive)
        {
            // Detect slope under the player
            Vector2 groundNormal = Vector2.up;
            float slopeAngle = 0f;
            if (isGrounded && groundCheck != null)
            {
                groundNormal = GetGroundNormal();
                slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            }

            bool onSlope = isGrounded && slopeAngle > slopeSlideAngleThreshold;

            if (onSlope)
            {
                Vector2 groundTangent = new Vector2(groundNormal.y, -groundNormal.x).normalized; // along-surface direction
                float currentMoveSpeed = isInGunk ? moveSpeed * gunkSlowFactor : moveSpeed;

                // Determine desired movement along the slope
                float dot = Vector2.Dot(groundTangent, Vector2.right);
                float signDot = Mathf.Sign(dot == 0f ? 1f : dot);
                Vector2 desiredAlongSlope = groundTangent * currentMoveInput * currentMoveSpeed * signDot;

                // Is the player trying to go uphill?
                bool tryingToGoUphill = Vector2.Dot(desiredAlongSlope, Vector2.up) > 0f;

                // Allow climb if a recent dash provided enough boost
                bool hasRecentDashClimb = (Time.time - lastDashReleaseTime) <= dashClimbWindow
                                          && lastDashBoostMagnitude * dashClimbStrengthFactor >= slopeAngle;

                if (Mathf.Abs(currentMoveInput) < moveDeadZone)
                {
                    // No input -> passive slide down
                    Vector2 downhill = -groundTangent;
                    float slideScale = Mathf.Clamp01(slopeAngle / maxSlopeAngle);
                    rb.linearVelocity = new Vector2(downhill.x * slideSpeed * slideScale, rb.linearVelocity.y);
                }
                else if (tryingToGoUphill && !hasRecentDashClimb)
                {
                    // Trying to go uphill but not enough boost -> force slide down instead
                    Vector2 downhill = -groundTangent;
                    float slideScale = Mathf.Clamp01(slopeAngle / maxSlopeAngle);
                    rb.linearVelocity = new Vector2(downhill.x * slideSpeed * slideScale, rb.linearVelocity.y);
                }
                else
                {
                    // Allow movement along slope (normal walking or dash-enabled climb)
                    rb.linearVelocity = new Vector2(desiredAlongSlope.x, rb.linearVelocity.y);
                }
            }
            else
            {
                // Flat ground or not grounded: normal horizontal movement
                float currentMoveSpeed = isInGunk ? moveSpeed * gunkSlowFactor : moveSpeed;
                if (moveAction != null && moveAction.enabled && isMovementEnabled && !isChargingDash)
                {
                    float moveVelocity = currentMoveInput * currentMoveSpeed;
                    // Blend input movement with landing momentum (momentum gradually decays)
                    float momentumBlend = Mathf.Clamp01(landingMomentumDecayTimer / landingMomentumDecayTime);
                    moveVelocity += landingMomentumX * momentumBlend;
                    rb.linearVelocity = new Vector2(moveVelocity, rb.linearVelocity.y);
                }
                else if (!isChargingDash)
                {
                    // Maintain momentum while no input
                    float momentumBlend = Mathf.Clamp01(landingMomentumDecayTimer / landingMomentumDecayTime);
                    float momentumVelocity = landingMomentumX * momentumBlend;
                    rb.linearVelocity = new Vector2(momentumVelocity, rb.linearVelocity.y);
                }
            }
        }
        
        // If the player is knocked and presses any key, transition to GettingUp
        if (isInKnocked && knockedPlayedOnce)
        {
            if (IsAnyInputPressed())
            {
                StartGettingUpSequence();
            }
        }

        // store vertical velocity for next-frame landing detection
        prevYVelocity = rb != null ? rb.linearVelocity.y : 0f;

        #endregion

        #region Jump
        // Charging jump while input is held
        if (isChargingJump)
        {
            ImpulseTimer -= Time.deltaTime;
            if (ImpulseTimer <= 0f)
            {
                isChargingJump = false;
                Jump();
            }
        }
        #endregion

        #region Mid-Air Boost (Dash)
        // Mid-air horizontal boost system
        if (!isGrounded && !isChargingJump)
        {
            float gravityFactor = isInGunk ? Mathf.Max(0f, gunkGravityFactor) : 1f;

            // Gravity adjustments while in air (preserve dash boost behavior)
            if (!isDashBoostActive)
            {
                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * gravityFactor * Time.deltaTime;
                }
                else if (rb.linearVelocity.y > 0 && !isChargingJump)
                {
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * gravityFactor * Time.deltaTime;
                }
            }
            // Start charging if holding left/right AND have dashes left AND cooldown passed
            if (currentMoveInput != 0f && !isChargingDash && dashesRemaining > 0 && Time.time > airStateStartTime + ledgeFallCooldown)
            {
                isChargingDash = true;
                dashChargeTimer = 0f;
                dashChargeDirection = currentMoveInput; // Capture direction at start
                dashesRemaining--; // Use a dash
                print($"Dash Start - Direction: {dashChargeDirection}, Dashes Left: {dashesRemaining}");
            }

            // Charge the boost
            if (isChargingDash)
            {
                dashChargeTimer += Time.deltaTime;
                
                // Slow down time while charging
                if (TImeMaster.Instance != null)
                {
                    TImeMaster.Instance.SetTimeScale(0.5f);
                }
                
                // Slow fall while charging
                rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, -dashFallSpeed));
                
                // Release if input is released or max charge time reached
                if (currentMoveInput == 0f || dashChargeTimer >= dashChargeMaxTime)
                {
                    print($"Dash: {Mathf.Clamp01(dashChargeTimer / dashChargeMaxTime):P0}");
                    ApplyDashBoost();
                    lastDashTime = Time.time;
                }
            }
        }
        else    
        {
            // Release the boost if charging and grounded or jump starts
            if (isChargingDash)
            {
                ApplyDashBoost();
                lastDashTime = Time.time;
            }
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (rb == null || !isInGunk)
            return;

        float maxGunkSpeed = Mathf.Abs(moveSpeed * Mathf.Clamp01(gunkSlowFactor));
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxGunkSpeed, maxGunkSpeed);

        if (!Mathf.Approximately(clampedX, rb.linearVelocity.x))
        {
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }
    }

    // Basic Movement
    // Switcher de input de movimento
    public void EnableMovement()
    {
        isMovementEnabled = true;
    }
    public void DisableMovement()
    {
        isMovementEnabled = false;
        // Stop horizontal movement immediately
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    public void ToggleMovement()
    {
        if (moveAction == null) return;
        print("Toggling Movement");
        isMovementEnabled = !isMovementEnabled;

        if (!isMovementEnabled)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // Jump
    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        DisableMovement(); // Desativa o movimento horizontal durante o carregamento do salto
        if (isGrounded || hasGunkJumpReset)
        {
            print("Jump Pressed!");
            isChargingJump = true; // start charging
            holdPlayedThisCharge = false; // allow hold animation to play once for this charge
            dashesRemaining = maxAirDashes; // Reset dashes on jump
            airStateStartTime = Time.time; // Dash immediately available when jumping

            if (!isGrounded)
            {
                hasGunkJumpReset = false;
            }
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if (isChargingJump)
        {
            isChargingJump = false;
            Jump();
        }
    }

    void Jump()
    {
        DisableMovement(); // Desativa o movimento horizontal durante o salto
        ImpulseTimer = ImpulseTimer + 1f;
        print("Jumped! Impulse: " + (ImpulseForce / ImpulseTimer));
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, ImpulseForce / ImpulseTimer); // Aplica a força do salto
        // Reset hold-play flag so next charge can play hold animation once
        holdPlayedThisCharge = false;
    }

    #region Dash / Mid-Air Boost
    private void ApplyDashBoost()
    {
        // Calculate boost based on charge time and stored direction
        float chargePercent = Mathf.Clamp01(dashChargeTimer / dashChargeMaxTime);
        float boostVelocity = dashChargeDirection * dashBoostForce * chargePercent;

        // Apply horizontal velocity
        rb.linearVelocity = new Vector2(boostVelocity, rb.linearVelocity.y);

        // Track the last dash magnitude and time for slope-climb checks
        lastDashBoostMagnitude = Mathf.Abs(boostVelocity);
        lastDashReleaseTime = Time.time;

        // Momentum and direction feedback moved to release trigger above

        // Reset dash state
        isChargingDash = false;
        dashChargeTimer = 0f;
        dashChargeDirection = 0f;
        isDashBoostActive = true; // Protect boost velocity for 1 frame

        // Restore normal time immediately
        if (TImeMaster.Instance != null)
        {
            TImeMaster.Instance.RestoreTimeScale();
        }
    }
    #endregion

    // Animation helpers (inspired by PlayerMovement)
    private void UpdateAnimation(float horizontalInput)
    {
        if (SpritePlayer == null)
            return;

        AnimationState next = GetAnimationState(horizontalInput);
        PlayAnimation(next);
    }

    // Raycast downwards from the groundCheck to determine the ground normal for slope handling.
    private Vector2 GetGroundNormal()
    {
        if (groundCheck == null)
            return Vector2.up;

        var origin = groundCheck.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, slopeCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            return hit.normal;
        }

        return Vector2.up;
    }

    private AnimationState GetAnimationState(float horizontalInput)
    {
        if (isChargingJump && isGrounded)
            return AnimationState.Hold;

        if (!isGrounded)
            return AnimationState.Jump;

        if (horizontalInput < -moveDeadZone)
            return AnimationState.Left;

        if (horizontalInput > moveDeadZone)
            return AnimationState.Right;

        return AnimationState.Idle;
    }

    private void PlayAnimation(AnimationState state)
    {
        if (SpritePlayer == null)
            return;

        if (hasAnimationState && state == currentAnimationState)
            return;

        // If we're in the Hold state and the hold animation was already played for this charge, skip playing it again
        if (state == AnimationState.Hold && holdPlayedThisCharge)
            return;

        currentAnimationState = state;
        hasAnimationState = true;

        if (state == AnimationState.Hold)
            holdPlayedThisCharge = true;

        string animationName = state switch
        {
            AnimationState.Left => leftAnimation,
            AnimationState.Right => rightAnimation,
            AnimationState.Hold => holdAnimation,
            AnimationState.Jump => jumpAnimation,
            _ => idleAnimation
        };

        SpritePlayer.Play(animationName);
    }

    // Special animation sequences
    private void TriggerBigFallSequence()
    {
        if (SpritePlayer == null) return;
        if (isInSpecialAnimation) return; // already handling a special animation

        somQueda.Play();
        print("somQueda played");

        isInSpecialAnimation = true;
        DisableMovement();
            StartCoroutine(PlayAnimationAndWait(bigFallAnimation, () =>
            {
                // After BigFall finishes, go to knocked state
                SpritePlayer.Play(knockedAnimation);
                isInKnocked = true;
                knockedPlayedOnce = false; // will set true after one cycle
                // keep movement disabled until GettingUp completes
                StartCoroutine(MarkKnockedPlayedOnce());
            }));
    }

    private void StartGettingUpSequence()
    {
        if (SpritePlayer == null) return;
        if (!isInKnocked) return;

        isInKnocked = false;
        // Play GettingUp and wait to finish
        StartCoroutine(PlayAnimationAndWait(gettingUpAnimation, () =>
        {
            // After getting up, return to idle and re-enable normal control
            SpritePlayer.Play(idleAnimation);
            isInSpecialAnimation = false;
            EnableMovement();
        }));
    }

    private System.Collections.IEnumerator PlayAnimationAndWait(string animName, System.Action onComplete)
    {
        SpritePlayer.Play(animName);
        // wait at least one frame for animator to update
        yield return null;

        // wait until the current state's normalizedTime >= 1 (animation finished)
        var layer = 0;
        while (true)
        {
            if (SpritePlayer == null) yield break;
            var state = SpritePlayer.GetCurrentAnimatorStateInfo(layer);
            if (state.IsName(animName) && state.normalizedTime >= 1f)
                break;
            yield return null;
        }

        onComplete?.Invoke();
    }

    private System.Collections.IEnumerator MarkKnockedPlayedOnce()
    {
        if (SpritePlayer == null) yield break;
        // wait one frame then wait until knocked animation has completed at least one cycle
        yield return null;
        var layer = 0;
        while (true)
        {
            if (SpritePlayer == null) yield break;
            var state = SpritePlayer.GetCurrentAnimatorStateInfo(layer);
            if (state.IsName(knockedAnimation) && state.normalizedTime >= 1f)
                break;
            yield return null;
        }
        knockedPlayedOnce = true;
    }

    private System.Collections.IEnumerator EnableMovementAfterDelay(float delaySeconds)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delaySeconds);
        // Disable the startup delay flag
        isStartupDelayActive = false;
        // Enable movement after the delay
        EnableMovement();
    }

    // Input System friendly query for "any key or button pressed this frame".
    private bool IsAnyInputPressed()
    {
        // Keyboard
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;

        // Mouse buttons
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
                return true;
        }

        // Gamepad
        var gp = UnityEngine.InputSystem.Gamepad.current;
        if (gp != null)
        {
            foreach (var c in gp.allControls)
            {
                if (c is UnityEngine.InputSystem.Controls.ButtonControl b && b.wasPressedThisFrame)
                    return true;
            }
        }

        // Also check the Jump action (or Move action) as a fallback
        if (jumpAction != null && jumpAction.triggered) return true;
        if (moveAction != null && moveAction.triggered) return true;

        return false;
    }

    private void OnDisable()
    {
        // Unregister animator from TimeMaster
        if (TImeMaster.Instance != null)
        {
            TImeMaster.Instance.UnregisterAnimator(SpritePlayer);
            TImeMaster.Instance.RestoreTimeScale();
        }
        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpStarted;
            jumpAction.canceled -= OnJumpCanceled;
            jumpAction.Disable();
        }
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
}
