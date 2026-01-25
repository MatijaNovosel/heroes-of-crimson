using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Multiply Y by -100 to convert world position to sorting order
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }
}