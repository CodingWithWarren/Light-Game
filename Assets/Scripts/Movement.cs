using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 velocity = Vector2.zero;
    private Rigidbody2D rb;

    public Vector2 flareForce = new Vector2(5, 5);
    public float flareTorque = 50;

    public GameObject flare;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * speed * Time.deltaTime, 0, 0);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody2D flareRb = Instantiate(this.flare, transform.position + new Vector3(-0.5f, 0.25f, 0), Quaternion.identity).GetComponent<Rigidbody2D>();
            //flareRb.AddForce(flareForce, ForceMode2D.Impulse);
            //flareRb.AddTorque(flareTorque);
        }
    }
}
