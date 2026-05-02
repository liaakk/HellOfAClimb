using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Animator SpritePlayer;
    
    [SerializeField] private float speed;
    [SerializeField] private float maxJumpHold = 1f;
    [SerializeField] private float jumpSpeed;

    [Header("Air Control Delay")]
    [SerializeField] private float airControlDelay = 0.2f;
    [SerializeField] private float airControlStrength = 4f;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;

    private Rigidbody2D body;

    private float jumpTimer;
    private bool isJumping;

    private float airControlTimer;
    private bool canControlAir;

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
        float horizontalInput = moveAction.ReadValue<float>();

        // ANIMAÇÕES
        if (Keyboard.current.aKey.isPressed)
        { 
            SpritePlayer.Play("left"); 
        }
        else if (Keyboard.current.dKey.isPressed)
        { 
            SpritePlayer.Play("right"); 
        }
        else if (Keyboard.current.spaceKey.isPressed)
        {
            SpritePlayer.Play("hold");
        }
        else
        {
            SpritePlayer.Play("idle"); 
        }

        //isso é pra evitar que o personagem deslize qnd vai pro canto da plataforma
        if (IsGrounded() && Mathf.Abs(moveAction.ReadValue<float>()) < 0.1f)
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        }

        // BLOQUEIO enquanto carrega salto
        if (jumpAction.IsPressed())
        {
            horizontalInput = 0;
        }

        // MOVIMENTO
        if (IsGrounded())
        {
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
        }
        else
        {
            // delay de controlo no ar
            if (canControlAir)
            {
                body.linearVelocity = new Vector2(
                    horizontalInput * speed,
                    body.linearVelocity.y
                );
            }
        }

        // INÍCIO DO SALTO
        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            isJumping = true;
            jumpTimer = 0f;
        }

        // CARREGAR SALTO
        if (jumpAction.IsPressed() && isJumping)
        {
            jumpTimer += Time.deltaTime * 2f;

            if (jumpTimer >= maxJumpHold)
            {
                jumpTimer = maxJumpHold;
                PerformJump();
            }
        }

        // SOLTAR SALTO
        if (jumpAction.WasReleasedThisFrame() && isJumping)
        {
            SpritePlayer.Play("jump");
            PerformJump();
        }

        // AIR CONTROL DELAY TIMER
        if (!IsGrounded())
        {
            airControlTimer += Time.deltaTime;

            if (airControlTimer >= airControlDelay)
            {
                canControlAir = true;
            }
        }
        else
        {
            airControlTimer = 0f;
            canControlAir = false;
        }
    }

    private void PerformJump()
    {
        isJumping = false;

        float chargePercent = jumpTimer / maxJumpHold;
        chargePercent = Mathf.Max(0.1f, chargePercent);

        // salto proporcional ao tempo de carregamento
        body.linearVelocity = new Vector2(0, jumpSpeed * chargePercent);

        // reset controlo aéreo
        canControlAir = false;
        airControlTimer = 0f;
    }

    private bool IsGrounded()
    {
        return Mathf.Abs(body.linearVelocity.y) < 0.01f;
    }
}