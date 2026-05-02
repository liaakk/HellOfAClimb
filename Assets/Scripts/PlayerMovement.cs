using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    
    public Animator SpritePlayer;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f; // velocidade do player no chão

    [Header("Salto")]
    [SerializeField] private float jumpForce = 20f; // força do salto (quao alto ele pula)
    [SerializeField] private float maxJumpHold = 1f; // quanto tempo pode carregar o salto

    [Header("Dash (Boost Lateral)")]
    [SerializeField] private float dashSpeed = 12f; // velocidade do boost
    [SerializeField] private float dashTime = 0.2f; // quanto tempo dura o boost

    [Header("Queda")]
    [SerializeField] private float fallMultiplier = 2f; // descida: quanto mais rápido cai
    [SerializeField] private float lowJumpMultiplier = 2f; // se soltar salto cedo, desce mais rápido

    [Header("Input")]
    [SerializeField] private InputAction moveAction; // A/D
    [SerializeField] private InputAction jumpAction; // ESPAÇO

    private Rigidbody2D body;

    // Variáveis de controlo de salto
    private float jumpTimer; // conta quanto tempo tá a carregar
    private bool isJumping; // está em modo salto?

    // Variáveis de controlo de dash
    private bool isDashing; // está a fazer boost?
    private float dashTimer; // conta quanto tempo falta pro dash acabar
    private float dashDirection; // para que lado vai (-1 esquerda, 1 direita)
    private bool hasUsedDashThisJump; // já fez dash neste salto? (impede mudar de direção)

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
        // Desativa os inputs quando o objeto é desligado
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // le o input
        float horizontalInput = moveAction.ReadValue<float>();

        // ==============================================================
        // ANIMAÇÕES - qual animação tocar
        // ==============================================================
        if (!IsGrounded()) // se está no ar
        {
            SpritePlayer.Play("jump"); 
        }
        else // se está no chão
        {
            if (horizontalInput < -0.1f) // vai para esquerda?
                SpritePlayer.Play("left");
            else if (horizontalInput > 0.1f) // vai para direita?
                SpritePlayer.Play("right");
            else // parado
                SpritePlayer.Play("idle");
        }

        // ==============================================================
        //  MOVIMENTO NO CHÃO
        // ==============================================================
        // Só move se está no chão E não está a fazer boost
        if (IsGrounded() && !isDashing)
        {
            body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocity.y);
            hasUsedDashThisJump = false; // reseta quando toca no chão (pronto pro próximo salto)
        }

        // ==============================================================
        //  SALTO (carregar para pular mais alto)
        // ==============================================================
        
        // Se pressionar ESPAÇO e está no chão, começa a carregar salto
        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            isJumping = true;
            jumpTimer = 0f;
            hasUsedDashThisJump = false; // reseta quando inicia novo salto (permite novo dash)
        }

        // Enquanto segura ESPAÇO, carrega o salto
        if (jumpAction.IsPressed() && isJumping)
        {
            jumpTimer += Time.deltaTime;

            // Se carregar tempo máximo, dispara já
            if (jumpTimer >= maxJumpHold)
                PerformJump();
        }

        // Quando solta ESPAÇO, faz o salto
        if (jumpAction.WasReleasedThisFrame() && isJumping)
        {
            PerformJump();
        }

        // ==============================================================
        // descida lenta antes do booost (para dar mais controle no ar)
        // ==============================================================
        // Só controla queda se não está a fazer boost
        if (!IsGrounded() && !isDashing)
        {
            // Se está a cair (velocidade Y negativa)
            if (body.linearVelocity.y < 0)
            {
                // Aumenta a gravidade = cai mais rápido
                body.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            
            else if (body.linearVelocity.y > 0 && !jumpAction.IsPressed())
            {
                
                body.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        // ==============================================================
        //  movimento reto pro lado
        // ==============================================================
        
        // Se está no ar E não está a fazer dash E ainda NÃO usou dash neste salto E pressiona A/D
        if (!IsGrounded() && !isDashing && !hasUsedDashThisJump)
        {
            if (Mathf.Abs(horizontalInput) > 0.1f) // se está a pressionar movimento
            {
                StartDash(horizontalInput); // começa o dash
            }
        }

        // Enquanto está a fazer dash
        if (isDashing)
        {
            dashTimer -= Time.deltaTime; // reduz o timer

            body.gravityScale = 0; //  desativa gravidade (movimento reto, so da pra fzr assim desativando a gravidade, se nn cai smp))
            body.linearVelocity = new Vector2(dashDirection * dashSpeed, 0); // move só para o lado

            // Se acabou o tempo do dash
            if (dashTimer <= 0)
            {
                isDashing = false;
                body.gravityScale = 1; // volta a gravidade normal
            }
        }
    }

    // Começa o boost lateral
    private void StartDash(float dir)
    {
        isDashing = true; // ativa modo dash
        dashTimer = dashTime; // reseta o timer
        dashDirection = Mathf.Sign(dir); 
        hasUsedDashThisJump = true; // marca como usado (impede mudar de direção)
    }

    // Executa o salto (com base na carga)
    private void PerformJump()
    {
        isJumping = false; // termina modo salto

        // Quanto mais tempo carregou, mais alto salta (0.2 a 1.0)
        float charge = jumpTimer / maxJumpHold;
        charge = Mathf.Max(0.2f, charge); // mínimo de 0.2 de carga

        // Aplica força vertical baseada na carga
        body.linearVelocity = new Vector2(0, jumpForce * charge);
    }

    // Checa se o player está no chão
    private bool IsGrounded()
    {
        // Se a velocidade vertical é quase 0, está no chão
        return Mathf.Abs(body.linearVelocity.y) < 0.01f;
    }
}