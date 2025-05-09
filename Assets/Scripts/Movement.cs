using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    [Header ("Speed")]
    public float speed = 5f;
    public float speedChangeRate = 0.1f;
    [SerializeField] float currentSpeedMultiplier = 1f;
    public float groundedSpeedMultiplier = 1f;
    public float jumpingSpeedMultiplier = 0.7f;

    [Header("Horizontal Force")]
    public float horizontalForce = 0f;
    [SerializeField] float horizontalForceDecreaseRate = 1.05f;

    [Header("Acceleration")]
    [SerializeField] float acceleration = 0f;
    public float accelerationTime = 0.15f;
    public bool halfAccelerationOnTurn = true;

    [Header("Jumping & Gravity")]
    public float jumpForce = 12f;
    public float gravityMultipler = 1f;
    public float terminalVelocity = -15f;

    [Header("Wall Actions")]
    public float wallJumpForce = 10f;
    public Vector2 wallKickVelocity = new Vector2(10, 5);
    [SerializeField] bool touchingWall = false;

    [Header("Circle Cast")]
    public float groundCastDistance = 0.3f;
    public float rightCastDistance = 0.15f;
    public float leftCastDistance = 0.12f;

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
        bool groundedHit = rb.CircleCast(Vector2.down, groundCastDistance);

        SetRotation();
        CheckForWallTouch();

        //Calculating Player State
        if (groundedHit)
        {
            state = PlayerStates.Grounded;

            LerpSpeedMultiplier(groundedSpeedMultiplier, speedChangeRate);

        }
        else if (state == PlayerStates.WallClinging)
        {
            //Stops velocity if wall clinging
            velocity = Vector2.zero;
        }
        else
        {
            state = PlayerStates.InAir;

            LerpSpeedMultiplier(jumpingSpeedMultiplier, speedChangeRate);
        }
    }

    private void FixedUpdate()
    {
        Gravity();
        CalculateAcceleration();
        CalculateForce();

        //Setting velocity.x if not wall clinging
        if (state != PlayerStates.WallClinging)
        {
            velocity.x = (speed * currentSpeedMultiplier * acceleration) + horizontalForce;
        }

        //Actually moving the player
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    #region Horizontal Movement

    private void LerpSpeedMultiplier(float target, float rate)
    {
        if (currentSpeedMultiplier != target)
        {
            //Linearly interpolating the speed multiplier
            currentSpeedMultiplier += (currentSpeedMultiplier - target < 0 ? rate : -rate) * Time.fixedDeltaTime;

            //Setting the speed multiplier to the target if the difference between the two is less than the chosen rate
            if (Math.Abs(currentSpeedMultiplier - target) < Math.Abs(rate * Time.fixedDeltaTime))
            {
                currentSpeedMultiplier = target;
            }
        }
    }

    public void SetRotation()
    {
        //Setting rotation based off of whether horizontal input is greather/less than zero
        if (velocity.x < 0 && transform.rotation == Quaternion.Euler(0, 0, 0))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        } else if (velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void CheckForWallTouch()
    {
        if (rb.CircleCast(Vector2.right, 0.14f))
        {
            velocity.x = 0f;
            acceleration = 0f;
            touchingWall = true;
        }
        else if (rb.CircleCast(Vector2.left, 0.14f))
        {
            velocity.x = 0f;
            acceleration = 0f;
            horizontalForce = horizontalForce < 0 ? 0 : horizontalForce;
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
            acceleration += moveActionXInput * (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
            acceleration = Mathf.Clamp(acceleration, -1, 1);
        } else 
        {
            //Calculating decceleration based on what direction the velocity is
            if (velocity.x > 0)
            {
                acceleration -= (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
                acceleration = Mathf.Clamp(acceleration, 0, 1);
            }
            else if (velocity.x < 0)
            {
                acceleration += (Time.fixedDeltaTime / (accelerationTime * accelerationMultiplier));
                acceleration = Mathf.Clamp(acceleration, -1, 0);
            }
        }
    }

    private void CalculateForce()
    {
        float forceDivider = horizontalForceDecreaseRate;

        //Decreases force faster if touching the ground
        if (state == PlayerStates.Grounded && Mathf.Abs(horizontalForce) > 0)
        {
            forceDivider += 0.1f;
        }

        //Checking for collisions with walls
        if (rb.CircleCast(Vector2.right, rightCastDistance) && horizontalForce > 0)
        {
            horizontalForce = 0;
        }
        else if (rb.CircleCast(Vector2.left, leftCastDistance) && horizontalForce < 0)
        {
            horizontalForce = 0;
        }

        //Calculating the force decrease
        if (Mathf.Abs(horizontalForce) > 1)
        {
            horizontalForce /= forceDivider;
        }
        else
        {
            horizontalForce = 0;
        }
    }
    #endregion

    #region Vertical Movement
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            if (state == PlayerStates.Grounded || rb.CircleCast(Vector2.down, groundCastDistance))
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
                    horizontalForce = -wallKickVelocity.x;
                    velocity.y = wallKickVelocity.y;
                } else if (playerHorizontalInput == 1 && Math.Abs(transform.rotation.eulerAngles.y) == 180)
                {
                    //Wall kicking to the right
                    horizontalForce = wallKickVelocity.x;
                    velocity.y = wallKickVelocity.y;
                } else
                {
                    //Wall Jumping
                    velocity.y = wallJumpForce;
                }

                state = PlayerStates.InAir;
                touchingWall = false;
            }
        }
    }

    public void WallClingInput(InputAction.CallbackContext context)
    {
        //if ((rb.CircleCast(Vector2.right, rightCastDistance) || rb.CircleCast(Vector2.left, leftCastDistance)) && context.performed && state == PlayerStates.InAir)
        //{
        //    state = PlayerStates.WallClinging;
        //}

        if (context.performed && state == PlayerStates.InAir)
        {
            if (rb.CircleCast(Vector2.right, rightCastDistance))
            {
                state = PlayerStates.WallClinging;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (rb.CircleCast(Vector2.left, leftCastDistance))
            {
                state = PlayerStates.WallClinging;
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    private void Gravity()
    {
        bool falling = velocity.y < 0 ? true : false;
        float multiplier = gravityMultipler;

        //Making gravity faster when falling
        if (falling || !jumpButton.IsPressed())
        {
            multiplier += 1.3f;
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