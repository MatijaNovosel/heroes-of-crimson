using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public Transform lookAt;

    public float boundX = 0.15f;
    public float boundY = 0.05f;

    [Header("Zoom")]
    public float minZoom = 6f;
    public float maxZoom = 20f;
    public float zoomSpeed = 2f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        HandleZoom();

        if (!lookAt) return;

        var delta = Vector3.zero;
        var deltaX = lookAt.position.x - transform.position.x;
        var deltaY = lookAt.position.y - transform.position.y;

        // Check if inside bound of X axis
        if (deltaX > boundX || deltaX < -boundX)
        {
            if (transform.position.x < lookAt.position.x)
            {
                delta.x = deltaX - boundX;
            }
            else
            {
                delta.x = deltaX + boundX;
            }
        }

        // Check if inside bound of Y axis
        if (deltaY > boundY || deltaY < -boundY)
        {
            if (transform.position.y < lookAt.position.y)
            {
                delta.y = deltaY - boundY;
            }
            else
            {
                delta.y = deltaY + boundY;
            }
        }

        transform.position += new Vector3(delta.x, delta.y, 0);
    }

    private void HandleZoom()
    {
        if (!cam) return;
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (!ctrlHeld) return;
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize,
            minZoom,
            maxZoom
        );
    }
}