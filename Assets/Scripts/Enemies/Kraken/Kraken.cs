using UnityEngine;

public class Kraken : MonoBehaviour
{
    public GameObject player;
    private SpriteRenderer spriteRenderer;
    private float deadZoneX = 0.05f;
    private BaseNPCBehaviour baseNPCBehaviour;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void LateUpdate()
    {
        var dx = player.gameObject.transform.position.x - transform.position.x;
        if (Mathf.Abs(dx) < deadZoneX) return;

        var playerIsRight = dx > 0f;

        spriteRenderer.flipX = !playerIsRight;
    }
}