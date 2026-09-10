using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float ySortOffset = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        float sortY = transform.position.y + ySortOffset;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(sortY * -100);
    }
}