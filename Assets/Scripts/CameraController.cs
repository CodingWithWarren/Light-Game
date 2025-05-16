using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    [Header("")]
    public float followDistance = 5;
    public float cameraSpeed;
    public float cameraMaxDistanceAway = 6;
    public float groundedYPosModifier = 1.5f;
    public float yPosModifierLerpRate = 1f;
    [SerializeField] private float yPosModifier = 0;
    [SerializeField] bool followingPlayer = true;

    private Vector2 playerPosition => new Vector2(player.transform.position.x, player.transform.position.y);
    private Vector2 cameraPosition => new Vector2(transform.position.x, transform.position.y);
    private float distanceToPlayer => Vector2.Distance(playerPosition, cameraPosition);
    Vector2 targetPosition => new Vector2(playerPosition.x, playerPosition.y + yPosModifier);
    float distanceFromTarget => Vector2.Distance(targetPosition, cameraPosition); //target position is just the player position with yPosModifier

    private Vector2 distanceToMove;

    private void Update()
    { 
        if (distanceFromTarget > followDistance && !followingPlayer)
        {
            followingPlayer = true;
        }

        yPosModifier = player.GetComponent<Movement>().state == Movement.PlayerStates.Grounded ? groundedYPosModifier : 0; //Setting the yPosModifier to grounded yPosModifier when grounded
    }

    private void LateUpdate()
    {
        if (followingPlayer)
        {
            if (distanceFromTarget >= cameraMaxDistanceAway)
            {
                distanceToMove = (distanceFromTarget - cameraMaxDistanceAway) * (playerPosition - cameraPosition).normalized; //The distance from the camera to cameraMaxDistanceAway away from the player
            } else
            {
                distanceToMove = (targetPosition - cameraPosition).normalized * cameraSpeed * Time.deltaTime;
            }

            transform.position += new Vector3(distanceToMove.x, distanceToMove.y, 0);

            if (distanceFromTarget < cameraSpeed * Time.deltaTime)
            {
                transform.position = new Vector3(playerPosition.x, playerPosition.y + yPosModifier, -10);
                followingPlayer = false;
            }
        }
    }
}
