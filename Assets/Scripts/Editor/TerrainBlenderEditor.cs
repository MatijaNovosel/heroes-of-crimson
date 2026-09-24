using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TerrainBlender))]
public class TerrainBlenderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var blender = (TerrainBlender)target;
        if (!Application.isPlaying && NeedsCapture(blender)) CapturePixels(blender, onlyMissing: true);

        DrawDefaultInspector();
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Add All Tiles From Source Tilemaps")) AddTilesFromSources(blender);
            if (GUILayout.Button("Recapture Tile Pixels")) CapturePixels(blender, onlyMissing: false);
        }

        DrawStatus(blender);
    }

    private static void DrawStatus(TerrainBlender blender)
    {
        int ready = 0;
        var problems = new List<string>();

        foreach (var terrain in blender.terrains)
        {
            if (!terrain.tile) continue;
            if (terrain.HasPixels) ready++;
            else problems.Add(terrain.tile.name);
        }

        if (problems.Count > 0)
        {
            EditorGUILayout.HelpBox(
                "Couldn't read pixels for: " + string.Join(", ", problems),
                MessageType.Warning
            );
        }
    }

    private static bool NeedsCapture(TerrainBlender blender)
    {
        foreach (var terrain in blender.terrains)
        {
            if (!terrain.tile) continue;
            var sprite = (terrain.tile as Tile)?.sprite;
            if (!terrain.HasPixels || terrain.capturedSprite != sprite) return true;
        }
        return false;
    }

    private static void AddTilesFromSources(TerrainBlender blender)
    {
        var existing = new HashSet<TileBase>();
        foreach (var terrain in blender.terrains)
        {
            if (terrain.tile) existing.Add(terrain.tile);
        }

        Undo.RecordObject(blender, "Add Blend Tiles");
        int added = 0;

        foreach (var tilemap in blender.sourceTilemaps)
        {
            if (!tilemap) continue;

            var used = new TileBase[tilemap.GetUsedTilesCount()];
            tilemap.GetUsedTilesNonAlloc(used);

            foreach (var tile in used)
            {
                if (!tile || !existing.Add(tile)) continue;
                blender.terrains.Add(new TerrainBlender.BlendTerrain { tile = tile });
                added++;
            }
        }

        CapturePixels(blender, onlyMissing: true);
        EditorUtility.SetDirty(blender);
        Debug.Log($"TerrainBlender: added {added} tile(s). Remove any that should keep hard edges.");
    }

    private static void CapturePixels(TerrainBlender blender, bool onlyMissing)
    {
        Undo.RecordObject(blender, "Capture Tile Pixels");
        var textureCache = new Dictionary<string, Texture2D>();

        foreach (var terrain in blender.terrains)
        {
            if (!terrain.tile) continue;
            var tile = terrain.tile as Tile;
            var sprite = tile ? tile.sprite : null;

            if (onlyMissing && terrain.HasPixels && terrain.capturedSprite == sprite) continue;

            terrain.pixels = null;
            terrain.size = 0;
            terrain.capturedSprite = null;
            if (!sprite) continue;

            var source = LoadReadableCopy(sprite.texture, textureCache);
            if (!source) continue;

            var rect = sprite.rect;
            int width = Mathf.RoundToInt(rect.width), height = Mathf.RoundToInt(rect.height);
            if (width != height) continue;

            var colors = source.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height);
            var pixels = new Color32[colors.Length];
            for (int i = 0; i < colors.Length; i++) pixels[i] = colors[i] * tile.color;

            terrain.pixels = pixels;
            terrain.size = width;
            terrain.capturedSprite = sprite;
        }

        foreach (var texture in textureCache.Values)
        {
            if (texture) Object.DestroyImmediate(texture);
        }

        EditorUtility.SetDirty(blender);
    }

    private static Texture2D LoadReadableCopy(Texture2D texture, Dictionary<string, Texture2D> cache)
    {
        if (!texture) return null;

        string path = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        if (cache.TryGetValue(path, out var cached)) return cached;

        var copy = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!copy.LoadImage(File.ReadAllBytes(path)))
        {
            Object.DestroyImmediate(copy);
            cache[path] = null;
            return null;
        }

        cache[path] = copy;
        return copy;
    }
}
