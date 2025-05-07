using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[RequireComponent (typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public enum PlayerStates
    {
        Grounded,
        InAir,
        WallClinging
    }
    [Header("General")]
    [SerializeField] PlayerStates state = PlayerStates.Grounded;
    [SerializeField] Vector2 velocity = Vector2.zero;

    private Rigidbody2D rb;

    [Header("Horizontal Movement")]
    public float speed = 5f;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float horizontalInput = 0f;

    [Header("Horizontal Momentum")]
    public float horizontalMomentum = 0f;
    [SerializeField] float horizontalMomentumDecreaseRate = 1.05f;

    [Header("Acceleration")]
    public float accelerationTime = 0.15f;
    public bool halfAccelerationOnTurn = true;

    [Header("Jumping & Gravity")]
    public float jumpForce = 12f;
    public float jumpingSpeedMultiplier = 0.7f;
    public float gravityMultipler = 1f;
    public bool gravityActivated = true;
    public float terminalVelocity = -15f;

    [Header("Wall Actions")]
    public bool wallActions = true;
    public float wallJumpForce = 10f;
    public Vector2 wallKickVelocity = new Vector2(10, 5);
    [SerializeField] bool touchingWall = false;

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

        HorizontalMovement();
        Gravity();

        //Calculating Player State
        if (groundedHit)
        {
            state = PlayerStates.Grounded;

            speedMultiplier = 1f;
            //GroundedMovement();

        }
        else if (state == PlayerStates.WallClinging)
        {
            //Stops gravity if wall clinging
            velocity.y = 0;
        }
        else
        {
            state = PlayerStates.InAir;

            speedMultiplier = jumpingSpeedMultiplier;
        }
    }

    private void FixedUpdate()
    {
        CalculateAcceleration();
        CalculateMomentum();

        //Setting velocity.x if not wall clinging
        if (state != PlayerStates.WallClinging)
        {
            velocity.x = (speed * speedMultiplier * horizontalInput) + horizontalMomentum;
        }

        if (state == PlayerStates.WallClinging)
        {
            //OnWallCling();
        }

        //Actually moving the player
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    #region Horizontal Movement

    public void HorizontalMovement()
    {
        //Setting rotation based off of whether velocity is greather/less than zero
        if (velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        } else if (velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        //Checking if touching a wall
        if (rb.CircleCast(Vector2.right, 0.14f))
        {
            velocity.x = 0f;
            horizontalInput = 0f;
            touchingWall = true;
        } else if (rb.CircleCast(Vector2.left, 0.14f))
        {
            velocity.x = 0f;
            horizontalInput = 0f;
            horizontalMomentum = horizontalMomentum < 0 ? 0 : horizontalMomentum;
            touchingWall = true;
        }
        else
        {
            touchingWall = false;
        }
    }

    private void CalculateAcceleration()
    {
        float moveActionXInput = moveAction.ReadValue<Vector2>().x;
        float accelerationMultiplier = 1;

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
    }

    private void CalculateMomentum()
    {
        float momentumDivider = horizontalMomentumDecreaseRate;

        //Decreases momentum faster if touching the ground
        if (state == PlayerStates.Grounded && Mathf.Abs(horizontalMomentum) > 0)
        {
            momentumDivider += 0.1f;
        }

        //Checking for collisions with walls
        if (rb.CircleCast(Vector2.right, 0.3f) && horizontalMomentum > 0)
        {
            print(horizontalMomentum);
            horizontalMomentum = 0;
        }
        else if (rb.CircleCast(Vector2.left, 0.3f) && horizontalMomentum < 0)
        {
            horizontalMomentum = 0;
            print(horizontalMomentum);
        }

        //Calculating the momentum decrease
        if (Mathf.Abs(horizontalMomentum) > 1)
        {
            horizontalMomentum /= momentumDivider;
        }
        else
        {
            horizontalMomentum = 0;
        }
    }
    #endregion

    #region Vertical Movement
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            if (state == PlayerStates.Grounded)
            {
                //Jumping
                velocity.y = jumpForce;
            }
            else if (state == PlayerStates.WallClinging)
            {
                float playerHorizontalInput = moveAction.ReadValue<Vector2>().x;

                if (playerHorizontalInput == -1 && transform.rotation.eulerAngles.y == 0)
                {
                    //Wall kicking to the left
                    horizontalMomentum = -wallKickVelocity.x;
                    velocity.y = wallKickVelocity.y;
                } else if (playerHorizontalInput == 1 && Math.Abs(transform.rotation.eulerAngles.y) == 180)
                {
                    //Wall kicking to the right
                    horizontalMomentum = wallKickVelocity.x;
                    velocity.y = wallKickVelocity.y;
                } else
                {
                    //Wall Jumping
                    velocity.y = wallJumpForce;
                }

                state = PlayerStates.InAir;
                touchingWall = false;
                gravityActivated = true;
            }
        }
    }

    public void WallClingInput(InputAction.CallbackContext context)
    {
        if ((rb.CircleCast(Vector2.right, 0.26f) || rb.CircleCast(Vector2.left, 0.3f)) && context.performed && state == PlayerStates.InAir)
        {
            state = PlayerStates.WallClinging;
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


        if (state == PlayerStates.InAir)
        {
            //Setting gravity
            velocity.y += Physics2D.gravity.y * multiplier * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        } else
        {
            velocity.y = Mathf.Max(velocity.y, 0f);
        }
    }
    #endregion

    //private void GroundedMovement()
    //{
    //    // Prevent gravity from infinitly building up
    //    //velocity.y = Mathf.Max(velocity.y, 0f);
    //}

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            touchingWall = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb.CircleCast(Vector2.up, 0.3f))
        {
            velocity.y = 0;
        }
    }
}