using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UIElements;

public class RoomEntrance : MonoBehaviour
{
    public GameObject nextEntrance;
    public GameObject blackScreen;
    public GameObject cameraFollowPlayerObject;

    private BoxCollider2D boxCollider;

    public bool active = true;
    private float transitionDuration = 0.4f;
    [SerializeField] private bool playerMovesLeft;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        List<Collider2D> overlaps = new List<Collider2D>();
        boxCollider.Overlap(overlaps);
        foreach (Collider2D overlap in overlaps)
        {
            if (overlap.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                active = false;
                break;
            }
        }

        playerMovesLeft = Mathf.Abs(transform.rotation.eulerAngles.y) == 0 ? true : false;
        print(Mathf.Abs(transform.rotation.eulerAngles.y));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            active = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (active)
            {
                //Cutscene and teleport
                StartCoroutine(Cutscene(collision.gameObject));
            }
        }
    }

    private IEnumerator Cutscene(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovesLeft)
        {
            playerMovement.ManualMove(-1);
        }
        else
        {
            playerMovement.ManualMove(1);
        }

        blackScreen.transform.position = new Vector2(cameraFollowPlayerObject.transform.position.x, cameraFollowPlayerObject.transform.position.y);

        nextEntrance.GetComponent<RoomEntrance>().active = false;
        blackScreen.LeanAlpha(1, transitionDuration);

        yield return new WaitForSeconds(transitionDuration);

        player.transform.position = nextEntrance.transform.position;

        yield return new WaitForSeconds(0.25f);

        blackScreen.LeanAlpha(0, transitionDuration);

        yield return new WaitForSeconds(transitionDuration - 0.25f);

        playerMovement.StopManualMovement();
    }
}