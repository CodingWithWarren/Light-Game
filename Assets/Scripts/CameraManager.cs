using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineCamera[] allCameras;

    [Header("Controls for lerping the Y damping during player jump/fall")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -2;

    public bool isLerpingYDamping { get; private set; }
    public bool lerpedFromPlayerFalling { get; set; }
    public bool isPlayerFalling = true;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer positionComposer;

    private float normYPanAmount;

    private Vector2 startingTargetOffset;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (currentCamera != null && currentCamera.gameObject.CompareTag("CenterPlayerFollowCam"))
        {
            positionComposer.enabled = false;
            Vector3 startPosition = new Vector3(-10.5477619f, 0.234449387f, -10.000001f);
            currentCamera.ForceCameraPosition(startPosition, Quaternion.identity);
            positionComposer.enabled = true;
        }
    }

    public void SetCurrentCamera(CinemachineCamera camera)
    {
        foreach (CinemachineCamera existingCamera in allCameras)
        {
            existingCamera.enabled = false;
        }

        //set current active camera
        camera.enabled = true; //enable it
        currentCamera = camera;

        //set positionComposer
        positionComposer = currentCamera.GetComponent<CinemachinePositionComposer>();

        //Set the normYPanAmount to the original damping
        normYPanAmount = positionComposer.Damping.y;
    }

    #region Lerp Y Damping

    public void LerpYDamping(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startDampAmount = positionComposer.Damping.y;
        float endDampAmount;

        //determine end damping
        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            lerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        LeanTween.value(startDampAmount, endDampAmount, fallYPanTime)
            .setOnUpdate((float value) => { positionComposer.Damping.y = value; })
            .setOnComplete( () => { 
                isLerpingYDamping = false; 
            } );
    }

    #endregion

    #region Pan Camera

    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        //handle pan to new position
        if (!panToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
                default:
                    break;
            }

            endPos *= panDistance;

            startPos = startingTargetOffset;

            endPos += startPos;
        }

        //handle pan to starting position
        else
        {
            startPos = positionComposer.TargetOffset;
            endPos = startingTargetOffset;
        }

        //actual panning
        GameObject go = new GameObject("pan filler");
        LeanTween.value(go, startPos, endPos, panTime)
            .setOnUpdate((Vector3 position) =>
            {
                positionComposer.TargetOffset = position;
            })
            .setOnComplete(() =>
            {
                Destroy(go);
            });
    }

    #endregion

    #region Swap Cameras

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection, PlayerMovement playerMovement)
    {
        //if the current camera was camera from the left and exit direction was from the right
        if (currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            //activate new camera
            cameraFromRight.enabled = true;

            //deactivate old camera
            cameraFromLeft.enabled = false;

            //set the new camera as current camera
            currentCamera = cameraFromRight;

            //update our position composer variable
            positionComposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
        }

        //if the current camera was camera from the right and exit direction was from the left
        if (currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            //activate new camera
            cameraFromLeft.enabled = true;

            //deactivate old camera
            cameraFromRight.enabled = false;

            //set the new camera as current camera
            currentCamera = cameraFromLeft;

            //update our position composer variable
            positionComposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
        }
    }

    #endregion
}
