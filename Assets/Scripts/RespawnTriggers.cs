using UnityEngine;

public class RespawnTriggers : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.GetComponent<PlayerManager>().respawnPosition = transform.position;
        }
    }
}
