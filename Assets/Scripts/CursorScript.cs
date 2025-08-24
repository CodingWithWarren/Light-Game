using UnityEngine;

public class CursorScript : MonoBehaviour
{
    private Camera mainCam;
    public PlayerActions playerActions;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        Cursor.visible = false; // Hide system cursor
        mainCam = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Keep it on the same plane
        transform.position = mousePos;

        if (playerActions.forcePercentage > 0)
        {
            if (!gameObject.LeanIsTweening() && playerActions.forcePercentage != 1)
            {
                float tweenTime = playerActions.maxHoldDuration;

                LeanTween.value(gameObject, Color.white, Color.red, tweenTime)
                    .setOnUpdate((Color val) =>
                    {
                        GetComponent<SpriteRenderer>().color = val;
                    });
            }
        } else
        {
            if (gameObject.LeanIsTweening())
            {
                gameObject.LeanCancel();
            }
            spriteRenderer.color = Color.white;
        }
    }
}
