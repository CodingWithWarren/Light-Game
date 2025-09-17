using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private PlayerMovement playerMovement;

    private bool isFacingRight;
    public float rotationTime = 0.5f;

    private void Awake()
    {
        playerMovement = playerTransform.GetComponent<PlayerMovement>();

        isFacingRight = playerMovement.isFacingRight;
    }

    private void Update()
    {
        transform.position = playerTransform.position;
    }

    public void CallTurn()
    {
        print("CallTurn");
        if (gameObject.LeanIsTweening())
        {
            gameObject.LeanCancel();
        }
        LeanTween.rotateY(gameObject, DetermineEndRotation(), rotationTime).setEaseInOutSine();
    }

    private float DetermineEndRotation()
    {
        if (playerMovement.isFacingRight)
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }
}
