using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    private const float FadeInTime = 0.12f;
    private const float FadeOutTime = 0.3f;

    private const float HeadGap = 0.1f;
    private const float StatusIconsTop = 1.25f;

    private SpeechBubbles _manager;
    private Transform _speaker;
    private Renderer _renderer;
    private bool _hasStatusIcons;

    private RectTransform _rect;
    private CanvasGroup _group;

    private float _hideTime;
    private float _alpha;
    private bool _dismissing;
    private Vector3 _lastAnchor;

    internal static SpeechBubble Create(
        SpeechBubbles manager,
        Transform speaker,
        string text,
        float duration,
        TMP_FontAsset font
    )
    {
        int s = SpeechBubbles.PixelScale;

        var root = new GameObject("SpeechBubble", typeof(RectTransform));
        var rect = (RectTransform)root.transform;
        rect.SetParent(manager.CanvasRect, false);
        rect.SetAsLastSibling();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0f);

        var bubble = root.AddComponent<SpeechBubble>();
        bubble._manager = manager;
        bubble._speaker = speaker;
        bubble._rect = rect;
        bubble._hideTime = Time.time + duration;
        bubble._group = root.AddComponent<CanvasGroup>();
        bubble._group.alpha = 0f;
        bubble._group.blocksRaycasts = false;
        bubble._group.interactable = false;

        bubble._renderer = speaker.GetComponent<SpriteRenderer>();
        if (!bubble._renderer) bubble._renderer = speaker.GetComponentInChildren<Renderer>();
        bubble._hasStatusIcons = speaker.GetComponent<BaseNPCBehaviour>();

        // Box
        var box = CreateRect("Box", rect);
        var boxImage = box.gameObject.AddComponent<Image>();
        boxImage.sprite = manager.BoxSprite;
        boxImage.type = Image.Type.Sliced;
        boxImage.raycastTarget = false;

        // Text
        var textRect = CreateRect("Text", box);
        var label = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font) label.font = font;
        label.fontSize = SpeechBubbles.FontSize;
        label.color = SpeechBubbles.TextColor;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.richText = true;
        label.raycastTarget = false;
        label.text = text;

        var preferred = label.GetPreferredValues(text, SpeechBubbles.MaxTextWidth, 0f);
        float textWidth = Mathf.Min(Mathf.Ceil(preferred.x), SpeechBubbles.MaxTextWidth);
        float textHeight = Mathf.Ceil(label.GetPreferredValues(text, textWidth, 0f).y);

        float insetX = s + SpeechBubbles.PaddingX;
        float insetY = s + SpeechBubbles.PaddingY;

        int columns = Mathf.CeilToInt((textWidth + insetX * 2f) / s);
        if (columns % 2 == 0) columns++;
        int rows = Mathf.CeilToInt((textHeight + insetY * 2f) / s);

        float boxWidth = columns * s;
        float boxHeight = rows * s;

        // Tail
        var tailSize = manager.TailSprite.rect.size;
        float tailVisible = tailSize.y - s;

        rect.sizeDelta = new Vector2(boxWidth, boxHeight + tailVisible);

        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0f);
        box.pivot = new Vector2(0.5f, 0f);
        box.anchoredPosition = new Vector2(0f, tailVisible);
        box.sizeDelta = new Vector2(boxWidth, boxHeight);

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(insetX, insetY);
        textRect.offsetMax = new Vector2(-insetX, -insetY);

        var tail = CreateRect("Tail", rect);
        tail.SetAsLastSibling();
        tail.anchorMin = tail.anchorMax = new Vector2(0.5f, 0f);
        tail.pivot = new Vector2(0.5f, 0f);
        tail.anchoredPosition = Vector2.zero;
        tail.sizeDelta = tailSize;
        var tailImage = tail.gameObject.AddComponent<Image>();
        tailImage.sprite = manager.TailSprite;
        tailImage.raycastTarget = false;

        bubble._lastAnchor = bubble.HeadPosition();
        bubble.UpdatePosition();
        return bubble;
    }

    public void Dismiss(bool immediate = false)
    {
        if (immediate)
        {
            _manager.Unregister(_speaker, this);
            Destroy(gameObject);
            return;
        }

        _dismissing = true;
    }

    private void LateUpdate()
    {
        if (_speaker) _lastAnchor = HeadPosition();
        else _dismissing = true;

        if (Time.time >= _hideTime) _dismissing = true;

        UpdatePosition();

        float target = _dismissing ? 0f : 1f;
        float fadeTime = _dismissing ? FadeOutTime : FadeInTime;
        _alpha = Mathf.MoveTowards(_alpha, target, Time.unscaledDeltaTime / fadeTime);
        _group.alpha = _alpha;

        if (_dismissing && _alpha <= 0f)
        {
            _manager.Unregister(_speaker, this);
            Destroy(gameObject);
        }
    }

    private Vector3 HeadPosition()
    {
        var position = _speaker.position;
        float top = _renderer ? _renderer.bounds.max.y : position.y + 0.5f;

        if (_hasStatusIcons) top = Mathf.Max(top, position.y + StatusIconsTop);

        return new Vector3(position.x, top + HeadGap, 0f);
    }

    private void UpdatePosition()
    {
        var cam = Camera.main;
        if (!cam) return;

        var screenPoint = cam.WorldToScreenPoint(_lastAnchor);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _manager.CanvasRect, 
                screenPoint, 
                null, 
                out var local
            )
        )
        {
            _rect.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y));
        }
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }
}
