using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorTransparencyController : MonoBehaviour
{
  public Tilemap floorTilemap;
  public Player player;
  public Color transparentColor = new Color(1f, 1f, 1f, 0.5f);
  public Color normalColor = Color.white;
  public int radius = 1;

  private Vector3Int lastTilePosition;

  void Update()
  {
    if (!player) return;
    
    var tilePosition = floorTilemap.WorldToCell(player.transform.position);

    if (tilePosition == lastTilePosition) return;
    
    // Reset
    for (var x = -radius; x <= radius; x++)
    {
      for (var y = -radius; y <= radius; y++)
      {
        var pos = lastTilePosition + new Vector3Int(x, y, 0);
        if (!floorTilemap.HasTile(pos)) continue;
        floorTilemap.SetTileFlags(pos, TileFlags.None);
        floorTilemap.SetColor(pos, normalColor);
      }
    }

    // Transparent tiles
    for (var x = -radius; x <= radius; x++)
    {
      for (var y = -radius; y <= radius; y++)
      {
        var pos = tilePosition + new Vector3Int(x, y, 0);
        if (!floorTilemap.HasTile(pos)) continue;
        floorTilemap.SetTileFlags(pos, TileFlags.None);
        floorTilemap.SetColor(pos, transparentColor);
      }
    }

    lastTilePosition = tilePosition;
  }
}