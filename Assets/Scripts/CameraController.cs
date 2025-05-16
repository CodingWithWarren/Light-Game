using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public float followDistance = 5;
    public float cameraSpeed;
    public float cameraMaxDistanceAway = 6;
    [SerializeField] bool followingPlayer = false;

    private Vector2 playerPosition => new Vector2(player.transform.position.x, player.transform.position.y);
    private Vector2 cameraPosition => new Vector2(transform.position.x, transform.position.y);
    private float distanceToPlayer => Vector2.Distance(playerPosition, cameraPosition);

    private Vector2 distanceToMove;

    private void Update()
    {
        print(Vector2.Distance(playerPosition, cameraPosition) > followDistance);
        if (distanceToPlayer > followDistance && !followingPlayer)
        {
            followingPlayer = true;
        }
    }

    private void LateUpdate()
    {
        if (followingPlayer)
        {
            if (distanceToPlayer >= cameraMaxDistanceAway)
            {
                distanceToMove += (distanceToPlayer - cameraMaxDistanceAway) * (playerPosition - cameraPosition).normalized;
            } else
            {
                distanceToMove = (playerPosition - cameraPosition).normalized * cameraSpeed * Time.deltaTime;
            }

            transform.position += new Vector3(distanceToMove.x, distanceToMove.y, 0);

            if (distanceToPlayer < cameraSpeed * Time.deltaTime)
            {
                transform.position = new Vector3(playerPosition.x, playerPosition.y, -10);
                followingPlayer = false;
            }
        }
    }
}
