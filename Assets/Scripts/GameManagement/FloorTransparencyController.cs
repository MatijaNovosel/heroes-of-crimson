using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorTransparencyController : MonoBehaviour
{
    public Tilemap[] floorTilemaps;
    public Player player;

    public Color transparentColor = new Color(1f, 1f, 1f, 0.5f);
    public Color normalColor = Color.white;

    public int radius = 1;

    private readonly Dictionary<Tilemap, Vector3Int> _lastTilePositions = new();

    private void Update()
    {
        if (!player || floorTilemaps == null)
            return;

        foreach (var floorTilemap in floorTilemaps)
        {
            if (!floorTilemap)
                continue;

            var tilePosition = floorTilemap.WorldToCell(player.transform.position);

            // Player hasn't changed tile on this tilemap.
            if (_lastTilePositions.TryGetValue(floorTilemap, out var lastPosition))
            {
                if (tilePosition == lastPosition)
                    continue;

                // Reset old transparent area.
                SetAreaColor(
                    floorTilemap,
                    lastPosition,
                    normalColor
                );
            }

            // Make new area transparent.
            SetAreaColor(
                floorTilemap,
                tilePosition,
                transparentColor
            );

            _lastTilePositions[floorTilemap] = tilePosition;
        }
    }

    private void SetAreaColor(
        Tilemap tilemap,
        Vector3Int center,
        Color color
    )
    {
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var pos = center + new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(pos))
                    continue;

                tilemap.SetTileFlags(pos, TileFlags.None);
                tilemap.SetColor(pos, color);
            }
        }
    }
}