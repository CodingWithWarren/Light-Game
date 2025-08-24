using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LightActivatedDoor : MonoBehaviour
{
    private GameObject doorPiece;
    private GameObject lightPiece;

    private int detectedLightCount = 0;

    private Vector3 originalDoorPosition;
    public Vector3 offset = new Vector3(0, 2.1f, 0);

    private const float detectionDelay = 0.1f;
    public float doorTweenTime = 0.5f;

    private bool doorOpen = false;

    private void Start()
    {
        doorPiece = gameObject.transform.parent.Find("Door").gameObject;
        lightPiece = gameObject.transform.parent.Find("Light").gameObject;
        originalDoorPosition = doorPiece.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            detectedLightCount++;

            if (!doorOpen)
            {
                StartCoroutine(OpenDoor());
                doorOpen = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            detectedLightCount--;

            if (doorOpen && detectedLightCount <= 0 && gameObject.activeSelf)
            {
                StartCoroutine(CloseDoor());
                doorOpen = false;
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(detectionDelay);
        lightPiece.SetActive(true);

        if (doorPiece.LeanIsTweening())
        {
            doorPiece.LeanCancel();
        }

        doorPiece.LeanMove(originalDoorPosition + offset, doorTweenTime)
                    .setEaseInExpo();
    }

    private IEnumerator CloseDoor()
    {
        lightPiece.SetActive(false);

        if (doorPiece.LeanIsTweening())
        {
            doorPiece.LeanCancel();
        }

        doorPiece.LeanMove(originalDoorPosition, doorTweenTime)
                    .setEaseInExpo();

        yield return null;
    }
}
