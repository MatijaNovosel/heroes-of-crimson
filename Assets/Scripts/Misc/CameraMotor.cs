using System.Collections;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public static CameraMotor Singleton { get; private set; }

    public Transform lookAt;
    public float boundX = 0.15f, boundY = 0.05f;

    [Header("Zoom")]
    public float minZoom = 6f, maxZoom = 20f, zoomSpeed = 2f;

    private float drunkDuration = 15f;
    private float rotationAmount = 25f;
    private float wobbleAmount = 0.55f;
    private float zoomAmount = 3f;
    private float effectSpeed = 3f;

    private Camera cam;
    private Coroutine drunkRoutine;
    private bool isDrunk;

    private float rotation;
    private float zoomOffset;
    private Vector2 wobble;

    private void Awake()
    {
        Singleton = this;
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        HandleZoom();
        FollowTarget();

        if (isDrunk)
        {
            transform.localRotation = Quaternion.Euler(0, 0, rotation);
            transform.position += (Vector3)wobble;
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void FollowTarget()
    {
        if (!lookAt) return;
        Vector3 delta = Vector3.zero;
        float x = lookAt.position.x - transform.position.x;
        float y = lookAt.position.y - transform.position.y;
        if (Mathf.Abs(x) > boundX) delta.x = x > 0 ? x - boundX : x + boundX;
        if (Mathf.Abs(y) > boundY) delta.y = y > 0 ? y - boundY : y + boundY;
        transform.position += delta;
    }

    private void HandleZoom()
    {
        if (!cam || isDrunk) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (!ctrl) return;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed,
            minZoom,
            maxZoom
        );
    }

    public void PlayDrunkEffect()
    {
        if (drunkRoutine != null) StopCoroutine(drunkRoutine);
        drunkRoutine = StartCoroutine(DrunkEffect());
    }

    private IEnumerator DrunkEffect()
    {
        isDrunk = true;

        float time = 0f;
        float baseZoom = cam.orthographicSize;

        float seed1 = Random.value * 1000f;
        float seed2 = Random.value * 1000f;
        float seed3 = Random.value * 1000f;

        while (time < drunkDuration)
        {
            time += Time.deltaTime;

            float t = time / drunkDuration;
            float intensity = Mathf.Sin(t * Mathf.PI);

            float Noise(float seed) => Mathf.PerlinNoise(seed, time * effectSpeed) * 2f - 1f;

            rotation = Noise(seed1) * rotationAmount * intensity;
            wobble = new Vector2(Noise(seed2), Noise(seed3)) * (wobbleAmount * intensity);
            zoomOffset = Noise(seed1 + 50f) * zoomAmount * intensity;

            cam.orthographicSize = Mathf.Clamp(
                baseZoom + zoomOffset,
                minZoom,
                maxZoom
            );

            yield return null;
        }

        rotation = 0f;
        wobble = Vector2.zero;
        cam.orthographicSize = baseZoom;

        isDrunk = false;
        drunkRoutine = null;
    }
}