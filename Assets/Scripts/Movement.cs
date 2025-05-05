using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[RequireComponent (typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField] Vector2 velocity = Vector2.zero;
    private Rigidbody2D rb;

    [Header("Horizontal Movement")]
    public float speed = 5f;
    private float speedMultiplier = 1f;
    public float horizontalMomentum = 0f;
    private float movementPressTime = 0f;
    [SerializeField] float horizontalInput = 0f;

    [Header("Acceleration")]
    public float accelerationTime = 0.1f;
    public bool halfAccelerationOnTurn = true;

    [Header("Jumping")]
    public float jumpForce = 7f;
    public float jumpingSpeedMultiplier;
    public float gravityMultipler = 1f;
    [SerializeField] bool canJump = true;
    [SerializeField] bool jumping = false;
    [SerializeField] bool grounded = true;

    [Header("")]
    public Vector2 flareForce = new Vector2(5f, 5f);
    public float flareTorque = 50f;

    public GameObject flare;

    public PlayerInput playerInput;
    private InputActionAsset inputActionAsset;
    private InputAction jumpButton;
    private InputAction moveAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActionAsset = playerInput.actions;
        jumpButton = inputActionAsset.FindActionMap("Player").FindAction("Jump");
        moveAction = inputActionAsset.FindActionMap("Player").FindAction("Move");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Rigidbody2D flareRb = Instantiate(this.flare, transform.position + new Vector3(-0.5f, 0.25f, 0), Quaternion.identity).GetComponent<Rigidbody2D>();
        }

        //Checking whether or not player is grounded
        bool groundedHit = rb.CircleCast(Vector2.down, 0.3f);
        if (!grounded && groundedHit && !jumping)
        {
            speedMultiplier = 1f;
            grounded = true;
        }

        HorizontalMovement();
        Gravity();

        if (groundedHit)
        {
            //Grounded movement when grounded
            GroundedMovement();
        } else
        {
            //Lowering horizontal movement speed when in the air
            speedMultiplier = jumpingSpeedMultiplier;
        }
    }

    private void FixedUpdate()
    {
        CalculateAcceleration();

        //Setting velocity.x
        velocity.x = (speed * speedMultiplier * horizontalInput) + horizontalMomentum;

        //Actually moving the player
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void CalculateAcceleration()
    {
        float moveActionXInput = moveAction.ReadValue<Vector2>().x;
        float accelerationMultiplier = 1;
        print(moveActionXInput);

        if (moveActionXInput != 0)
        {
            //Halving the acceleration on turn
            if (((moveActionXInput < 0 && velocity.x > 0) || (moveActionXInput > 0 && velocity.x < 0)) && halfAccelerationOnTurn)
            {
                accelerationMultiplier = 0.5f;
            } else
            {
                accelerationMultiplier = 1;
            }

            //Calculating acceleration and clamping it
            horizontalInput += moveActionXInput * (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
            horizontalInput = Mathf.Clamp(horizontalInput, -1, 1);
        } else 
        {
            //Calculating Decceleration based on what direction the velocity is
            if (velocity.x > 0)
            {
                horizontalInput -= (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
                horizontalInput = Mathf.Clamp(horizontalInput, 0, 1);
            }
            else if (velocity.x < 0)
            {
                horizontalInput += (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
                horizontalInput = Mathf.Clamp(horizontalInput, -1, 0);
            }
        }

        //horizontalInput += moveActionXInput * (Time.fixedDeltaTime / accelerationTime);
    }

    public void HorizontalMovement()
    {
        //Setting rotation based off of whether velocity is greather/less than zero
        transform.rotation = Quaternion.Euler(0, velocity.x < 0 ? 180 : 0, 0);

        //Decreasing horizontal momentum
        horizontalMomentum = horizontalMomentum > 1 ? (horizontalMomentum <= 5 ? horizontalMomentum / 1.05f : horizontalMomentum / 1.01f) : 0;

        if (rb.CircleCast(Vector2.right * velocity.normalized.x, 0.3f))
        {
            velocity.x = 0f;
            horizontalInput = 0f;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            //Actual jump
            velocity.y = jumpForce;

            grounded = false;
            jumping = true;
        }
    }

    private void Gravity()
    {
        bool falling = velocity.y < 0 ? true : false;
        float multiplier = gravityMultipler;

        //Making gravity faster when falling
        if (falling | !jumpButton.IsPressed())
        {
            multiplier = 1.3f;
        }

        velocity.y += Physics2D.gravity.y * multiplier * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(velocity.y, -15);
    }

    private void GroundedMovement()
    {
        // Prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;
    }
}
