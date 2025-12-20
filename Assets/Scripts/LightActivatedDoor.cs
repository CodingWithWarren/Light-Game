using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightActivatedDoor : MonoBehaviour
{
    private GameObject doorPiece;
    private GameObject lightPiece;

    private GameObject symbol;
    private SpriteRenderer symbolSpriteRenderer;
    private Light2D symbolLight;

    private GameObject doorIndicator;
    private SpriteRenderer doorIndicatorSpriteRenderer;
    private Light2D doorIndicatorLight;

    private int detectedLightCount = 0;

    private Vector3 originalDoorPosition;
    public Vector3 offset = new Vector3(0, 2.1f, 0);

    private const float detectionDelay = 0.1f;
    public float doorTweenTime = 0.5f;

    private bool doorOpen = false;

    [Header("Visual")]
    public Sprite DetectorOffSprite;
    public Sprite DetectorOnSprite;
    public Sprite DoorOffSprite;
    public Sprite DoorOnSprite;

    private ParticleSystem onParticle;
    private ParticleSystem offParticle;

    private void Start()
    {
        doorPiece = gameObject.transform.parent.Find("Door").gameObject;
        lightPiece = gameObject.transform.parent.Find("Light").gameObject;
        originalDoorPosition = doorPiece.transform.position;

        symbol = gameObject.transform.GetChild(0).gameObject;
        symbolSpriteRenderer = symbol.GetComponent<SpriteRenderer>();
        symbolLight = symbol.GetComponent<Light2D>();

        doorIndicator = gameObject.transform.parent.GetChild(0).GetChild(0).gameObject;
        doorIndicatorSpriteRenderer = doorIndicator.GetComponent<SpriteRenderer>();
        doorIndicatorLight = doorIndicator.GetComponent<Light2D>();

        onParticle = transform.parent.Find("On Particle").GetComponent<ParticleSystem>();
        offParticle = transform.parent.Find("Off Particle").GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Incremeneting light count every time a light collides (what nah)
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
        //Decrementing light count every time a light leaves
        if (collision.gameObject.tag == "Light")
        {
            detectedLightCount--;

            //Closing door if there are no lights
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

        symbolSpriteRenderer.sprite = DetectorOnSprite;
        symbolLight.lightCookieSprite = DetectorOnSprite;
        symbolLight.blendStyleIndex = 1;

        onParticle.gameObject.transform.position = transform.position;
        onParticle.Play();

        doorIndicatorSpriteRenderer.sprite = DoorOnSprite;
        doorIndicatorLight.lightCookieSprite = DoorOnSprite;
        doorIndicatorLight.blendStyleIndex = 1;

        lightPiece.SetActive(true);

        if (doorPiece.LeanIsTweening())
        {
            doorPiece.LeanCancel();
        }

        //Open door anim
        doorPiece.LeanMove(originalDoorPosition + offset, doorTweenTime)
                    .setEaseInExpo();
    }

    private IEnumerator CloseDoor()
    {
        lightPiece.SetActive(false);

        symbolSpriteRenderer.sprite = DetectorOffSprite;
        symbolLight.lightCookieSprite = DetectorOffSprite;
        symbolLight.blendStyleIndex = 0;

        offParticle.gameObject.transform.position = transform.position;
        offParticle.Play();

        doorIndicatorSpriteRenderer.sprite = DoorOffSprite;
        doorIndicatorLight.lightCookieSprite = DoorOffSprite;
        doorIndicatorLight.blendStyleIndex = 0;

        if (doorPiece.LeanIsTweening())
        {
            doorPiece.LeanCancel();
        }

        //Close door anim
        doorPiece.LeanMove(originalDoorPosition, doorTweenTime)
                    .setEaseInExpo();

        yield return null;
    }
}
