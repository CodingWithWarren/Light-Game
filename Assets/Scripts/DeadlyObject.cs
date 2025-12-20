using System.Collections;
using UnityEngine;

public class DeadlyObject : MonoBehaviour
{
    private bool killActive = true;

    public const float playerForceMagnitude = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && killActive)
        {
            StartCoroutine(KillPlayer(collision));
        }
    }

    private IEnumerator KillPlayer(Collider2D collision)
    {
        Vector2 direction = (collision.transform.position - transform.position).normalized;
        direction.y += 0.5f;

        killActive = false;
        yield return StartCoroutine(collision.gameObject.GetComponent<PlayerManager>().Die(direction * playerForceMagnitude));
        killActive = true;
    }
}
