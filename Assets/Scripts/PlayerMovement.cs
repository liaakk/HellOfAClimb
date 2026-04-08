using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    
    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    
    private Rigidbody2D body;

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

        // Check if the jump button is currently being held down
        if (jumpAction.IsPressed())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, speed);
        }
    }
}
