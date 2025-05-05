using System.Collections;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float flickerIntensity = 0.25f;
    public float flickerInterval = 1f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(Flicker());
    }

    private void Update()
    {
        if (transform.parent != null)
        {
            transform.rotation = Quaternion.Euler(0, 0, -transform.parent.rotation.z);
        }
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            float flicker = Random.Range(originalScale.x + flickerIntensity, originalScale.x - flickerIntensity);
            transform.localScale = new Vector3(flicker, flicker, flicker);

            yield return new WaitForSeconds(flickerInterval);
        }
    }
}
