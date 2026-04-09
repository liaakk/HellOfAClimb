using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float maxJumpHold = 1f;
    [SerializeField] private float jumpSpeed;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    
    private Rigidbody2D body;
    private float jumpTimer;
    private bool isJumping;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>(); 
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Read the horizontal input (expecting a 1D Axis binding)
        float horizontalInput = moveAction.ReadValue<float>();
        
        body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);

        // Start charging the jump when the button is first pressed
        if (jumpAction.WasPressedThisFrame())
        {
            isJumping = true;
            jumpTimer = 0f;
        }

        // While the button is held, increase the timer
        if (jumpAction.IsPressed() && isJumping)
        {
            jumpTimer += Time.deltaTime;

            // Automatically jump if the max time is reached
            if (jumpTimer >= maxJumpHold)
            {
                jumpTimer = maxJumpHold;
                PerformJump();
            }
        }

        // Execute the jump when the player releases the button early
        if (jumpAction.WasReleasedThisFrame() && isJumping)
        {
            PerformJump();
        }
    }

    private void PerformJump()
    {
        isJumping = false;

        // Calculate jump strength based on charge time (0.0 to 1.0)
        float chargePercent = jumpTimer / maxJumpHold;
        
        // Ensure a minimal jump height (e.g., 20% of max) even for a quick tap
        chargePercent = Mathf.Max(0.2f, chargePercent);

        // Use jumpSpeed instead of speed
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed * chargePercent);
    }
}
