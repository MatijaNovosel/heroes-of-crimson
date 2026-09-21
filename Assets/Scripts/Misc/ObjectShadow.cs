using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    private Vector3 localPosition = new(0f, -0.8f, 0f);
    private Vector3 localScale = new(1f, -0.6f, 1f);
    private Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    private string sortingLayer = "Actor";
    private int sortingOrder = 5;

    private void Awake()
    {
        CreateShadow();
    }

    private void CreateShadow()
    {
        Transform existingShadow = transform.Find("Shadow");

        if (existingShadow != null) return;

        GameObject shadow = new GameObject("Shadow");

        shadow.transform.SetParent(transform);

        shadow.transform.localPosition = localPosition;
        shadow.transform.localRotation = Quaternion.identity;
        shadow.transform.localScale = localScale;

        SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        SpriteRenderer parentRenderer = GetComponent<SpriteRenderer>();
        if (parentRenderer != null) shadowRenderer.sprite = parentRenderer.sprite;

        shadowRenderer.color = shadowColor;
        shadowRenderer.sortingLayerName = sortingLayer;
        shadowRenderer.sortingOrder = sortingOrder;
    }
}