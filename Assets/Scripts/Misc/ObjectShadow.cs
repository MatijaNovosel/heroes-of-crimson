using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    private const string ShadowName = "Shadow";

    [Header("Shape")] 
    [SerializeField] private float maxShadowHeight = 0.6f;
    private Color _shadowColor = new(0f, 0f, 0f, 0.35f);
    private readonly float _squash = 0.5f;

    private SpriteRenderer parentRenderer, shadowRenderer;
    private Transform shadowTransform;
    private Sprite lastSprite;

    private void Awake()
    {
        parentRenderer = GetComponent<SpriteRenderer>();
        CreateShadow();
    }

    private void CreateShadow()
    {
        if (parentRenderer == null || parentRenderer.sprite == null || transform.Find(ShadowName) != null) return;
        shadowTransform = new GameObject(ShadowName).transform;
        shadowTransform.SetParent(transform, false);
        shadowRenderer = shadowTransform.gameObject.AddComponent<SpriteRenderer>();
        shadowRenderer.color = _shadowColor;
        ApplyShape();
        ApplySorting();
    }

    public void SetVisible(bool visible)
    {
        if (shadowRenderer != null) shadowRenderer.enabled = visible;
    }

    private void LateUpdate()
    {
        if (shadowRenderer == null || parentRenderer == null) return;
        if (parentRenderer.sprite != lastSprite) ApplyShape();
        shadowRenderer.flipX = parentRenderer.flipX;
        ApplySorting();
    }

    private void ApplyShape()
    {
        lastSprite = shadowRenderer.sprite = parentRenderer.sprite;
        if (lastSprite == null) return;
        Bounds bounds = lastSprite.bounds;
        float s = maxShadowHeight > 0f && bounds.size.y > 0f ? Mathf.Min(_squash, maxShadowHeight / bounds.size.y) : _squash;
        shadowTransform.localPosition = new Vector3(0, bounds.min.y * (1f + s), 0f);
        shadowTransform.localScale = new Vector3(1f, -s, 1f);
    }

    private void ApplySorting()
    {
        shadowRenderer.sortingLayerName = "Actor";
    }
}