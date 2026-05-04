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

    [Header("Dash Settings")]
    public int maxAirDashes = 1; // Max dashes per jump
    public float dashChargeMaxTime = 1f;
    public float dashBoostForce = 15f;
    public float dashFallSpeed = 0.5f; // slow descent while charging
    public float ledgeFallCooldown = 0.2f; // Delay before dash is usable when falling off ledge

    [Header("Queda")]
    [SerializeField] private float fallMultiplier = 2f; // descida: quanto mais rápido cai
    [SerializeField] private float lowJumpMultiplier = 2f; // se soltar salto cedo, desce mais rápido

    [Header("Animações")]
    [SerializeField] private string idleAnimation = "idle";
    [SerializeField] private string leftAnimation = "left";
    [SerializeField] private string rightAnimation = "right";
    [SerializeField] private string holdAnimation = "hold";
    [SerializeField] private string jumpAnimation = "jump";
    [SerializeField] private float moveDeadZone = 0.1f;

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
        groundCheckCollider = groundCheck.GetComponent<BoxCollider2D>();
        
        if (groundCheckCollider == null)
            Debug.LogError("NovoMovimento: groundCheck child does not have a BoxCollider2D! Ground detection will not work.");
        
        initialImpulseTimer = ImpulseTimer;
        
        // Register animator with TimeMaster for time scale syncing
        if (TImeMaster.Instance != null)
        {
            TImeMaster.Instance.RegisterAnimator(SpritePlayer);
        }
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
        
        // Reset dashes when grounded
        if (isGrounded && !wasGroundedLastFrame)
        {
            dashesRemaining = maxAirDashes;
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
        }
        
        // Read movement input
        if (moveAction != null && moveAction.enabled)
        {
            currentMoveInput = moveAction.ReadValue<Vector2>().x;
        }

        // Update animations now that we have grounded state and input
        UpdateAnimation(currentMoveInput);

        if (!isGrounded)
        {
            print("Airborne");
        }
        // Ground: Enable movement
        if (isGrounded)
        {
            print("Grounded");
            if (!isChargingJump)
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
            if (moveAction != null && moveAction.enabled && isMovementEnabled && !isChargingDash)
            {
                rb.linearVelocity = new Vector2(currentMoveInput * moveSpeed, rb.linearVelocity.y);
            }
            else if (!isChargingDash)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
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
            // Gravity adjustments while in air (preserve dash boost behavior)
            if (!isDashBoostActive)
            {
                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                }
                else if (rb.linearVelocity.y > 0 && !isChargingJump)
                {
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
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
                
                print($"Dash Charging: {dashChargeTimer:F2}s / {dashChargeMaxTime:F2}s");
                
                // Release if input is released or max charge time reached
                if (currentMoveInput == 0f || dashChargeTimer >= dashChargeMaxTime)
                {
                    print($"Dash Release Triggered - Timer: {dashChargeTimer:F2}s, Max: {dashChargeMaxTime:F2}s, Direction: {dashChargeDirection}");
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
        if (isGrounded)
        {
            print("Jump Pressed!");
            isChargingJump = true; // start charging
            holdPlayedThisCharge = false; // allow hold animation to play once for this charge
            dashesRemaining = maxAirDashes; // Reset dashes on jump
            airStateStartTime = Time.time; // Dash immediately available when jumping
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

        print($"Dash Released! Charge: {chargePercent:P0}, Boost: {boostVelocity:F2}, Direction: {dashChargeDirection}");

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
