using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[RequireComponent (typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField] Vector2 velocity = Vector2.zero;
    private Rigidbody2D rb;

    public float speed = 5f;
    private float speedMultiplier = 1f;
    public float jumpingSpeedMultiplier;
    public float horizontalMomentum = 0f;
    public float accelerationMultiplier = 1.5f;

    public float jumpForce = 7f;
    public float gravityMultipler = 1f;
    [SerializeField] bool canJump = true;
    [SerializeField] bool jumping = false;

    [SerializeField] bool grounded = true;

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
            //flareRb.AddForce(flareForce, ForceMode2D.Impulse);
            //flareRb.AddTorque(flareTorque);
        }

        bool groundedHit = rb.CircleCast(Vector2.down, 0.3f);

        if (!grounded && groundedHit && !jumping)
        {
            speedMultiplier = 1f;
            grounded = true;
        }

        //HorizontalMovement();

        Gravity();

        print(groundedHit);
        if (groundedHit)
        {
            speedMultiplier = 1f;
            GroundedMovement();
        } else
        {
            speedMultiplier = jumpingSpeedMultiplier;
        }
    }

    private void FixedUpdate()
    {
        velocity.x = (speed * speedMultiplier * moveAction.ReadValue<Vector2>().x) + horizontalMomentum;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void HorizontalMovement(InputAction.CallbackContext context)
    {
        print("function called");

        //Setting rotation based off of whether velocity is greather/less than zero
        transform.rotation = Quaternion.Euler(0, velocity.x < 0 ? 180 : 0, 0);

        //Decreasing horizontal momentum
        horizontalMomentum = horizontalMomentum > 1 ? (horizontalMomentum <= 5 ? horizontalMomentum / 1.05f : horizontalMomentum / 1.01f) : 0;

        if (rb.CircleCast(Vector2.right * velocity.normalized.x, 0.3f))
        {
            velocity.x = 0f;
        }
    }

    private void ChangeHorizontalMovement()
    {
        velocity.x = (speed * speedMultiplier * moveAction.ReadValue<Vector2>().x) + horizontalMomentum;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            grounded = false;
            velocity.y = jumpForce;
            jumping = true;
            speedMultiplier = jumpingSpeedMultiplier;
        }
    }

    private void Gravity()
    {
        bool falling = velocity.y < 0 ? true : false;
        float multiplier = gravityMultipler;

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
