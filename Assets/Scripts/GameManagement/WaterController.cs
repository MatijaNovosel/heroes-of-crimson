using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterController : MonoBehaviour
{
    public Tilemap waterTilemap;
    public Player player;
    public PlayerWaterMask waterMask;
    private SpriteRenderer playerRenderer;

    void Start()
    {
        if (player)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (!player) return;

        Vector3Int cellPos = waterTilemap.WorldToCell(player.transform.position);
        bool onWater = waterTilemap.HasTile(cellPos);

        if (onWater)
        {
            waterMask.gameObject.SetActive(true);
            Vector3 playerPos = player.gameObject.transform.position;
            playerPos.y -= 0.5f;
            waterMask.gameObject.transform.position = playerPos;
        }
        else
        {
            waterMask.gameObject.SetActive(false);
        }
    }
}