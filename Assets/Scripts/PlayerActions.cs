using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] public GameObject flarePrefab;

    public float defaultForce = 7f;
    public float maxForce = 12f;

    public float forcePercentage { get; private set; } = 0;

    private const float minHoldDuration = 0.1f;
    public float maxHoldDuration { get; private set; } = 0.5f; 

    private float startTime = 0;

    private Camera mainCam;
    private GameObject flareParentObject;

    void Start()
    {
        mainCam = Camera.main;
        flareParentObject = GameObject.Find("Flares");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            startTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float time = Time.time - startTime;
            ThrowFlare();
            startTime = 0;
        }

        if (startTime != 0)
        {
            float time = Time.time - startTime;
            if (time > minHoldDuration)
            {
                forcePercentage = Mathf.Clamp01((time - minHoldDuration) / maxHoldDuration);
            }
        } else
        {
            forcePercentage = 0;
        }
    }

    //private float CalculateForce(float time)
    //{
    //    if (time >= minHoldDuration)
    //    {
    //        //Setting time to be a value between 0 and maxHoldDuration, while accounting for minHoldDuration
    //        time = Mathf.Clamp(time, minHoldDuration, minHoldDuration + maxHoldDuration) - minHoldDuration;

    //        float percentage = time / maxHoldDuration;
    //        return percentage * maxForce;
    //    } else
    //    {
    //        return defaultForce;
    //    }
    //}

    void ThrowFlare()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direction = (mousePos - transform.position).normalized;

        GameObject projectile = Instantiate(flarePrefab, transform.position, Quaternion.identity, flareParentObject.transform);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * Mathf.Clamp(forcePercentage * maxForce, defaultForce, maxForce); //Makes force at least the min force
        }
    }
}
