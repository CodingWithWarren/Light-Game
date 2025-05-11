using UnityEngine;

public class CursorScript : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        Cursor.visible = false; // Hide system cursor
        mainCam = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Keep it on the same plane
        transform.position = mousePos;
    }
}
