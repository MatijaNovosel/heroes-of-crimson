using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class FloorTransparency : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Color transparentColor = new(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color normalColor = Color.white;

    [SerializeField] private int radius = 1;

    private Tilemap _tilemap;
    private Vector3Int _lastTilePosition;
    private bool _hasLastPosition;

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();

        if (!player)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject) player = playerObject.GetComponent<Player>();
        }
    }

    private void Update()
    {
        if (!player) return;

        Vector3Int tilePosition = _tilemap.WorldToCell(player.transform.position);

        if (_hasLastPosition && tilePosition == _lastTilePosition) return;

        if (_hasLastPosition) SetAreaColor(_lastTilePosition, normalColor);

        SetAreaColor(tilePosition, transparentColor);

        _lastTilePosition = tilePosition;
        _hasLastPosition = true;
    }

    private void SetAreaColor(Vector3Int center, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int position = center + new Vector3Int(x, y, 0);
                if (!_tilemap.HasTile(position)) continue;
                _tilemap.SetTileFlags(position, TileFlags.None);
                _tilemap.SetColor(position, color);
            }
        }
    }

    private void OnDisable()
    {
        if (_hasLastPosition) SetAreaColor(_lastTilePosition, normalColor);
        _hasLastPosition = false;
    }
}