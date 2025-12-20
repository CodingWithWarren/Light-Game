using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class RoomEntrance : MonoBehaviour
{
    public string connectedSceneName;
    public CinemachineCamera connectedCamera;

    public bool caveStart;
    public GameObject blackScreen;

    private BoxCollider2D boxCollider;

    public bool checkForPlayer = true;
    private float transitionDuration = 0.4f;

    public enum Direction
    {
        Right, 
        Left,
        UpRight,
        UpLeft,
        Down
    };
    public Direction playerEnterDirection = Direction.Right;

    //Setting checkForPlayer to true when player leaves the collision box
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            checkForPlayer = true;
        }
    }

    //Exiting room on touch
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (checkForPlayer)
            {
                //Cutscene and teleport
                StartCoroutine(ExitRoom(collision.gameObject));
            }
        }
    }

    private IEnumerator ExitRoom(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        GameData.Instance.previousSceneName = SceneManager.GetActiveScene().name;

        //save the flare positions to be reloaded when the player comes back
        GameData.Instance.SaveFlarePositionsForCurrentScene();

        //Moving player manually based off direction
        switch (playerEnterDirection)
        {
            case Direction.Left:
                playerMovement.ManualMove(-1);
                break;

            case Direction.Right:
                playerMovement.ManualMove(1);
                break;

            case Direction.Down:
                playerMovement.DisableMovement();
                break;
        }

        //doing black screen thingy
        blackScreen.SetActive(true);
        blackScreen.transform.position = new Vector2(player.transform.position.x, player.transform.position.y);
        blackScreen.LeanAlpha(1, transitionDuration);

        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(connectedSceneName);
    }

    public IEnumerator EnterRoom(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        CameraManager.instance.SetCurrentCamera(connectedCamera);

        //making sure the black screen is active cuz i disactivate it cuz its super annoying ts pmo
        blackScreen.SetActive(true);
        blackScreen.GetComponent<SpriteRenderer>().color = Color.black;

        //spawn all flares that were there
        GameData.Instance.LoadSavedFlares();

        //Manually moving player
        switch (playerEnterDirection)
        {
            case Direction.Left:
                playerMovement.ManualMove(1);
                break;

            case Direction.Right:
                playerMovement.ManualMove(-1);
                break;

            case Direction.Down:
                playerMovement.DisableMovement();
                playerMovement.SetManualVelocity(new Vector2(-3, 50), 0, true);
                break;
        }

        yield return new WaitForSeconds(0.25f); //hides the camera moving to the player

        //black screen thingy
        blackScreen.LeanAlpha(0, transitionDuration);

        yield return new WaitForSeconds(transitionDuration);

        playerMovement.StopManualMovement();

        if (caveStart)
        {
            Destroy(gameObject);
        }
    }

    //start coroutine wasn't working :(
    public void Enter(GameObject player)
    {
        StartCoroutine(EnterRoom(player));
    }
}