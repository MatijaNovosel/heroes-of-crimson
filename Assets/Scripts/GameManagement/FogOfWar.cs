using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Tilemap fogTilemap;

    [Header("Fog Settings")]
    [SerializeField] private int revealRadius = 5;

    private Vector3Int lastPlayerCell;

    private void Start()
    {
        if (!fogTilemap) fogTilemap = GetComponent<Tilemap>();
        lastPlayerCell = fogTilemap.WorldToCell(player.position);
        RevealAroundPlayer();
    }

    private void Update()
    {
        if (!player || !fogTilemap) return;
        Vector3Int currentPlayerCell = fogTilemap.WorldToCell(player.position);
        if (currentPlayerCell != lastPlayerCell)
        {
            lastPlayerCell = currentPlayerCell;
            RevealAroundPlayer();
        }
    }

    private void RevealAroundPlayer()
    {
        Vector3Int playerCell = fogTilemap.WorldToCell(player.position);
        int radiusSquared = revealRadius * revealRadius;

        for (int x = -revealRadius; x <= revealRadius; x++)
        {
            for (int y = -revealRadius; y <= revealRadius; y++)
            {
                if (x * x + y * y > radiusSquared) continue;
                Vector3Int cell = new Vector3Int(
                    playerCell.x + x,
                    playerCell.y + y,
                    playerCell.z
                );
                RevealTile(cell);
            }
        }
    }

    private void RevealTile(Vector3Int cell)
    {
        if (!fogTilemap.HasTile(cell)) return;
        fogTilemap.SetTileFlags(cell, TileFlags.None);
        Color color = fogTilemap.GetColor(cell);
        color.a = 0f;
        fogTilemap.SetColor(cell, color);
    }
}