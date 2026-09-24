using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainBlender : MonoBehaviour
{
    [Serializable]
    public class BlendTerrain
    {
        public TileBase tile;

        [HideInInspector] public Color32[] pixels;
        [HideInInspector] public int size;
        [HideInInspector] public Sprite capturedSprite;

        public bool HasPixels => pixels != null && size > 0 && pixels.Length == size * size;
    }

    public List<Tilemap> sourceTilemaps = new();
    public List<BlendTerrain> terrains = new();

    // Border shaping
    public float warpAmount = 0.6f;
    public float warpFrequency = 0.45f;
    public int warpOctaves = 4;

    private readonly int _seed = 1337;
    public int chunkSize = 32;
    public bool bakeOnStart = true;
    
    private readonly List<GameObject> _chunkObjects = new();
    private readonly List<Texture2D> _chunkTextures = new();

    private BoundsInt _bounds;
    private int[] _cellTerrain; // index into terrains, -1 = empty, -2 = not blendable
    private int[] _cellSource; // index into sourceTilemaps of the topmost tile
    private Vector2 _warpOffsetX, _warpOffsetY;

    private void Start()
    {
        if (bakeOnStart) Bake();
    }

    private void OnDestroy()
    {
        ClearBake();
    }

    public void Bake()
    {
        ClearBake();
        if (sourceTilemaps.Count == 0 || terrains.Count == 0) return;

        var terrainIndex = BuildTerrainLookup(out int tileSize);
        if (terrainIndex == null) return;

        CollectCells(terrainIndex);
        if (_cellTerrain == null) return;

        var random = new System.Random(_seed);
        _warpOffsetX = new Vector2(random.Next(1000, 9000) + 0.37f, random.Next(1000, 9000) + 0.71f);
        _warpOffsetY = new Vector2(random.Next(1000, 9000) + 0.13f, random.Next(1000, 9000) + 0.59f);

        for (int cy = _bounds.yMin; cy < _bounds.yMax; cy += chunkSize)
        for (int cx = _bounds.xMin; cx < _bounds.xMax; cx += chunkSize)
        {
            BakeChunk(cx, cy, tileSize);
        }

        HideOriginalTiles();
    }

    private Dictionary<TileBase, int> BuildTerrainLookup(out int tileSize)
    {
        tileSize = 0;
        var lookup = new Dictionary<TileBase, int>();

        for (int i = 0; i < terrains.Count; i++)
        {
            var terrain = terrains[i];
            if (!terrain.tile || !terrain.HasPixels) continue;
            if (tileSize == 0) tileSize = terrain.size;
            else if (terrain.size != tileSize) continue;
            lookup[terrain.tile] = i;
        }

        if (lookup.Count == 0) return null;
        return lookup;
    }

    private void CollectCells(Dictionary<TileBase, int> terrainIndex)
    {
        bool first = true;
        foreach (var tilemap in sourceTilemaps)
        {
            if (!tilemap) continue;
            tilemap.CompressBounds();
            var b = tilemap.cellBounds;
            if (b.size.x == 0 || b.size.y == 0) continue;

            if (first) { _bounds = b; first = false; continue; }

            var min = Vector3Int.Min(_bounds.min, b.min);
            var max = Vector3Int.Max(_bounds.max, b.max);
            _bounds.SetMinMax(new Vector3Int(min.x, min.y, 0), new Vector3Int(max.x, max.y, 1));
        }

        if (first) { _cellTerrain = null; return; }

        int w = _bounds.size.x, h = _bounds.size.y;
        _cellTerrain = new int[w * h];
        _cellSource = new int[w * h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var cell = new Vector3Int(_bounds.xMin + x, _bounds.yMin + y, 0);
            int index = y * w + x;
            _cellTerrain[index] = -1;
            _cellSource[index] = -1;

            for (int s = 0; s < sourceTilemaps.Count; s++)
            {
                var tilemap = sourceTilemaps[s];
                if (!tilemap) continue;

                var tile = tilemap.GetTile(cell);
                if (!tile) continue;

                _cellTerrain[index] = terrainIndex.TryGetValue(tile, out int t) ? t : -2;
                _cellSource[index] = s;
                break;
            }
        }
    }


    private void BakeChunk(int chunkX, int chunkY, int tileSize)
    {
        int tilesW = Mathf.Min(chunkSize, _bounds.xMax - chunkX);
        int tilesH = Mathf.Min(chunkSize, _bounds.yMax - chunkY);

        if (!ChunkHasBlendCells(chunkX, chunkY, tilesW, tilesH)) return;

        int pixelsW = tilesW * tileSize, pixelsH = tilesH * tileSize;

        var layers = new Color32[sourceTilemaps.Count][];
        var used = new bool[sourceTilemaps.Count];

        for (int ty = 0; ty < tilesH; ty++)
        for (int tx = 0; tx < tilesW; tx++)
        {
            int cellX = chunkX + tx, cellY = chunkY + ty;
            int ownIndex = CellIndex(cellX, cellY);
            int ownTerrain = _cellTerrain[ownIndex];
            if (ownTerrain < 0) continue;

            for (int py = 0; py < tileSize; py++)
            for (int px = 0; px < tileSize; px++)
            {
                float u = cellX + (px + 0.5f) / tileSize;
                float v = cellY + (py + 0.5f) / tileSize;

                int chosenIndex = ownIndex;
                if (warpAmount > 0f)
                {
                    float dx = warpAmount * Fbm(u * warpFrequency + _warpOffsetX.x, v * warpFrequency + _warpOffsetX.y, warpOctaves);
                    float dy = warpAmount * Fbm(u * warpFrequency + _warpOffsetY.x, v * warpFrequency + _warpOffsetY.y, warpOctaves);
                    int sampled = CellIndex(Mathf.FloorToInt(u + dx), Mathf.FloorToInt(v + dy));
                    if (sampled >= 0 && _cellTerrain[sampled] >= 0) chosenIndex = sampled;
                }

                var terrain = terrains[_cellTerrain[chosenIndex]];
                Color32 color = terrain.pixels[py * tileSize + px];

                int layer = _cellSource[chosenIndex];
                layers[layer] ??= new Color32[pixelsW * pixelsH];
                used[layer] = true;
                layers[layer][(ty * tileSize + py) * pixelsW + tx * tileSize + px] = color;
            }
        }

        for (int s = 0; s < sourceTilemaps.Count; s++)
        {
            if (used[s]) CreateChunkObject(s, chunkX, chunkY, pixelsW, pixelsH, tileSize, layers[s]);
        }
    }

    private bool ChunkHasBlendCells(int chunkX, int chunkY, int tilesW, int tilesH)
    {
        for (int y = 0; y < tilesH; y++)
        for (int x = 0; x < tilesW; x++)
        {
            if (_cellTerrain[CellIndex(chunkX + x, chunkY + y)] >= 0) return true;
        }
        return false;
    }

    private void CreateChunkObject(int sourceIndex, int chunkX, int chunkY, int pixelsW, int pixelsH, int tileSize, Color32[] pixels)
    {
        var source = sourceTilemaps[sourceIndex];
        var sourceRenderer = source.GetComponent<TilemapRenderer>();

        var texture = new Texture2D(pixelsW, pixelsH, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = $"TerrainChunk_{source.name}_{chunkX}_{chunkY}"
        };
        
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        float pixelsPerUnit = tileSize / source.layoutGrid.cellSize.x;
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, pixelsW, pixelsH),
            Vector2.zero,
            pixelsPerUnit,
            0, 
            SpriteMeshType.FullRect
        );

        var go = new GameObject(texture.name);
        go.transform.SetParent(transform, false);
        go.transform.position = source.CellToWorld(new Vector3Int(chunkX, chunkY, 0));

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        
        if (sourceRenderer)
        {
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.sortingOrder = sourceRenderer.sortingOrder;
            renderer.sharedMaterial = sourceRenderer.sharedMaterial;
        }

        _chunkObjects.Add(go);
        _chunkTextures.Add(texture);
    }

    private void HideOriginalTiles()
    {
        int w = _bounds.size.x, h = _bounds.size.y;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (_cellTerrain[y * w + x] < 0) continue;

            var cell = new Vector3Int(_bounds.xMin + x, _bounds.yMin + y, 0);

            foreach (var tilemap in sourceTilemaps)
            {
                if (!tilemap) continue;
                var tile = tilemap.GetTile(cell);
                if (!tile || !IsBlendTile(tile)) continue;
                tilemap.SetTileFlags(cell, TileFlags.None);
                tilemap.SetColor(cell, Color.clear);
            }
        }
    }

    private bool IsBlendTile(TileBase tile)
    {
        foreach (var terrain in terrains)
        {
            if (terrain.tile == tile && terrain.HasPixels) return true;
        }
        return false;
    }

    private void ClearBake()
    {
        foreach (var go in _chunkObjects)
        {
            if (go) Destroy(go);
        }
        foreach (var texture in _chunkTextures)
        {
            if (texture) Destroy(texture);
        }
        _chunkObjects.Clear();
        _chunkTextures.Clear();
    }

    private int CellIndex(int cellX, int cellY)
    {
        int x = cellX - _bounds.xMin, y = cellY - _bounds.yMin;
        if (x < 0 || y < 0 || x >= _bounds.size.x || y >= _bounds.size.y) return -1;
        return y * _bounds.size.x + x;
    }
    
    private static float Fbm(float x, float y, int octaves)
    {
        float sum = 0f, amplitude = 1f, frequency = 1f, norm = 0f;
        for (int i = 0; i < octaves; i++)
        {
            sum += amplitude * (Mathf.PerlinNoise(x * frequency, y * frequency) - 0.5f) * 2f;
            norm += amplitude;
            amplitude *= 0.55f;
            frequency *= 2.1f;
        }
        return sum / norm;
    }
}