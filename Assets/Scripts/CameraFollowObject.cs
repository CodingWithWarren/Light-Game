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
        transform.position = playerMovement.transform.position;
    }

    private void Update()
    {
        transform.position = playerTransform.position;
    }

    //Tweening camera to face player's right when player turns
    public void CallTurn()
    {
        if (gameObject.LeanIsTweening())
        {
            gameObject.LeanCancel();
        }
        LeanTween.rotateY(gameObject, DetermineEndRotation(), rotationTime).setEaseInOutSine();
    }
    
    //i wonder
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
