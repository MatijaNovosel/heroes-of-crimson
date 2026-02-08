using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterController : MonoBehaviour
{
    public Tilemap waterTilemap;
    public Player player;
    public PlayerWaterMask waterMask;
    public SpriteRenderer playerShadow;

    void Update()
    {
        if (!player) return;

        Vector3Int cellPos = waterTilemap.WorldToCell(player.transform.position);
        bool onWater = waterTilemap.HasTile(cellPos);

        if (onWater)
        {
            playerShadow.enabled = false;
            waterMask.gameObject.SetActive(true);
            Vector3 playerPos = player.gameObject.transform.position;
            playerPos.y -= 0.5f;
            waterMask.gameObject.transform.position = playerPos;
        }
        else
        {
            playerShadow.enabled = true;
            waterMask.gameObject.SetActive(false);
        }
    }
}