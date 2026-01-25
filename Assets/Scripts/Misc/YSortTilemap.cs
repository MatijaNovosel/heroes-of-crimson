using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class YSortTilemap : MonoBehaviour
{
    private TilemapRenderer tileMapRenderer;

    void Awake()
    {
        tileMapRenderer = GetComponent<TilemapRenderer>();
    }

    void LateUpdate()
    {
        // Multiply Y by -100 to convert world position to sorting order
        tileMapRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }
}