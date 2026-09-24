using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbles : MonoBehaviour
{
    public const int PixelScale = 3;
    public const float MaxTextWidth = 280f;
    public const float FontSize = 22f;
    public const float PaddingX = 10f;
    public const float PaddingY = 5f;

    public static readonly Color32 FillColor = new(218, 215, 211, 255);
    public static readonly Color32 BorderColor = new(255, 255, 255, 255);
    public static readonly Color TextColor = new(84 / 255f, 84 / 255f, 84 / 255f, 1f);

    public static TMP_FontAsset FontOverride;
    private static SpeechBubbles _instance;
    private readonly Dictionary<Transform, SpeechBubble> _active = new();

    internal RectTransform CanvasRect { get; private set; }
    internal Sprite BoxSprite { get; private set; }
    internal Sprite TailSprite { get; private set; }
    
    public static SpeechBubble Say(Transform speaker, string text, float duration = -1f)
    {
        if (!speaker || string.IsNullOrWhiteSpace(text)) return null;

        var manager = GetOrCreate();

        if (manager._active.TryGetValue(speaker, out var existing) && existing)
        {
            existing.Dismiss(immediate: true);
        }

        if (duration <= 0f)
        {
            duration = Mathf.Clamp(2.5f + text.Length * 0.05f, 3f, 8f);
        }

        var bubble = SpeechBubble.Create(manager, speaker, text, duration, ResolveFont());
        manager._active[speaker] = bubble;
        return bubble;
    }

    public static void Clear(Transform speaker)
    {
        if (!_instance || !speaker) return;
        if (_instance._active.TryGetValue(speaker, out var bubble) && bubble) bubble.Dismiss();
    }

    internal void Unregister(Transform speaker, SpeechBubble bubble)
    {
        if (_active.TryGetValue(speaker, out var current) && current == bubble)
        {
            _active.Remove(speaker);
        }
    }

    private static TMP_FontAsset ResolveFont()
    {
        if (FontOverride) return FontOverride;

        if (PlayerLog.Singleton && PlayerLog.Singleton.playerLogItemPrefab)
        {
            var logText = PlayerLog.Singleton.playerLogItemPrefab.GetComponent<TMP_Text>();
            if (logText && logText.font) return logText.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private static SpeechBubbles GetOrCreate()
    {
        if (_instance) return _instance;
        var go = new GameObject("SpeechBubbles", typeof(RectTransform));
        _instance = go.AddComponent<SpeechBubbles>();
        _instance.Build();
        return _instance;
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -2;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0f;
        scaler.referencePixelsPerUnit = 100f;

        CanvasRect = (RectTransform)transform;

        BoxSprite = BuildSprite(new[]
        {
            ".BBB.",
            "BFFFB",
            "BFFFB",
            "BFFFB",
            ".BBB.",
        }, sliceBorder: 2);

        TailSprite = BuildSprite(new[]
        {
            "BFFFB",
            ".BFB.",
            "..B..",
        }, sliceBorder: 0);
    }

    private static Sprite BuildSprite(string[] rows, int sliceBorder)
    {
        int height = rows.Length;
        int width = rows[0].Length;

        var texture = new Texture2D(width * PixelScale, height * PixelScale, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "SpeechBubbleArt"
        };

        var pixels = new Color32[texture.width * texture.height];
        var clear = new Color32(0, 0, 0, 0);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                var color = rows[row][col] switch
                {
                    'B' => BorderColor,
                    'F' => FillColor,
                    _ => clear
                };

                int baseY = (height - 1 - row) * PixelScale;
                int baseX = col * PixelScale;

                for (int y = 0; y < PixelScale; y++)
                for (int x = 0; x < PixelScale; x++)
                {
                    pixels[(baseY + y) * texture.width + baseX + x] = color;
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        float border = sliceBorder * PixelScale;
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border)
        );
    }
}
