using System.Collections.Generic;
using Foundation;
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
    private readonly Dictionary<Vector3Int, List<Landmark>> landmarksByCell = new();

    private void Start()
    {
        if (!fogTilemap) fogTilemap = GetComponent<Tilemap>();
        CacheLandmarks();
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

    private void CacheLandmarks()
    {
        Landmark[] landmarks = FindObjectsOfType<Landmark>(true);

        foreach (Landmark landmark in landmarks)
        {
            Vector3Int cell = fogTilemap.WorldToCell(landmark.transform.position);

            if (!landmarksByCell.ContainsKey(cell))
            {
                landmarksByCell[cell] = new List<Landmark>();
            }

            landmarksByCell[cell].Add(landmark);
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
                RevealLandmarks(cell);
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

    private void RevealLandmarks(Vector3Int cell)
    {
        if (!landmarksByCell.TryGetValue(cell, out List<Landmark> landmarks)) return;
        foreach (Landmark landmark in landmarks)
        {
            if (landmark && !landmark.gameObject.activeSelf)
            {
                landmark.gameObject.SetActive(true);
            }
        }
    }
}