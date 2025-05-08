using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class Flare : MonoBehaviour
{
    [SerializeField] private Vector2 velocity = Vector2.zero;
    public Vector2 startVelocity = new Vector2(6, 6);

    public float bounceYForce;
    public float bounceXForce;
    public float maxBounces = 3;

    private float bounces = 0;

    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        velocity = startVelocity;

        bounceXForce = startVelocity.x / 2;
        bounceYForce = startVelocity.y / 2;
    }
    private void FixedUpdate()
    {
        velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (bounces < maxBounces)
            {
                velocity.y = bounceYForce;
                velocity.x = velocity.x < 0 ? -bounceXForce : bounceXForce;

                bounces++;
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            velocity.x *= -1;
        }
    }
}
