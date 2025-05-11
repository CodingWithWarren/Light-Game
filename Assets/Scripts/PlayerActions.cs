using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowRange = 5f; // max cursor distance that affects speed

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            ThrowFlare();
        }
    }

    void ThrowFlare()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direction = (mousePos - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, mousePos);

        // Normalize and clamp distance between 0 and 1
        float t = Mathf.Clamp01(distance / maxThrowRange);
        float scaledForce = Mathf.Lerp(throwForce * 0.3f, throwForce, t);

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * scaledForce;
        }
    }
}
