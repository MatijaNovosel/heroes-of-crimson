using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    private const float BarWidth = 800f;
    private const float BarHeight = 26f;
    private const float BorderSize = 6f;
    private const float NameTopOffset = 22f;
    private const float BarTopOffset = 92f;
    private const float NameFontSize = 40f;

    private static readonly Color PanelColor = new(48 / 255f, 40 / 255f, 40 / 255f);
    private static readonly Color BackgroundColor = new(14 / 255f, 12 / 255f, 12 / 255f);
    private static readonly Color FillColor = new(0.4056604f, 0.16264686f, 0.16264686f);
    private static readonly Color FillLightColor = new(0.6415094f, 0.21484515f, 0.21484515f);
    private static readonly Color DamageTrailColor = Color.white;

    private const float LightFillBottom = 0.23f;
    private const float DamageHoldTime = 0.3f;
    private const float DamageShrinkTime = 0.5f;

    private const float FadeTime = 0.4f;

    private BaseNPCBehaviour _boss;
    private CanvasGroup _canvasGroup;
    private RectTransform _fill;
    private RectTransform _fillLight;
    private RectTransform _damageTrail;

    private float _fillAmount;
    private float _trailAmount;
    private float _trailStartAmount;
    private float _lastDamageTime = -999f;

    private float _alpha;
    private bool _bossGone;

    public static BossHealthBar Create(BaseNPCBehaviour boss, string bossName, TMP_FontAsset font)
    {
        var root = new GameObject("BossHealthBar", typeof(RectTransform));
        var bar = root.AddComponent<BossHealthBar>();
        bar.Build(bossName, font);
        bar.SetBoss(boss);
        return bar;
    }

    private void SetBoss(BaseNPCBehaviour boss)
    {
        _boss = boss;
        _fillAmount = CurrentFraction();
        _trailAmount = _fillAmount;
        ApplyFill();
    }

    private void Update()
    {
        if (!_bossGone && !_boss)
        {
            _bossGone = true;
            OnHpChanged(0f);
        }
        else if (!_bossGone)
        {
            float fraction = CurrentFraction();
            if (!Mathf.Approximately(fraction, _fillAmount)) OnHpChanged(fraction);
        }

        UpdateDamageTrail();
        ApplyFill();
        UpdateFade();
    }

    private float CurrentFraction()
    {
        return Mathf.Clamp01(Mathf.Max(0f, _boss.hp) / Mathf.Max(1f, _boss.maxHp));
    }

    private void OnHpChanged(float newFraction)
    {
        if (newFraction < _fillAmount)
        {
            _trailStartAmount = Mathf.Max(_trailAmount, _fillAmount);
            _lastDamageTime = Time.unscaledTime;
        }
        else
        {
            _trailAmount = newFraction;
        }

        _fillAmount = newFraction;
    }

    private void UpdateDamageTrail()
    {
        if (_trailAmount <= _fillAmount)
        {
            _trailAmount = _fillAmount;
            return;
        }

        float t = (Time.unscaledTime - _lastDamageTime - DamageHoldTime) / DamageShrinkTime;
        if (t <= 0f) return;

        _trailAmount = Mathf.Lerp(_trailStartAmount, _fillAmount, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
    }

    private void UpdateFade()
    {
        bool visible = !_bossGone || _trailAmount > 0.001f;
        float target = visible ? 1f : 0f;

        _alpha = Mathf.MoveTowards(_alpha, target, Time.unscaledDeltaTime / FadeTime);
        _canvasGroup.alpha = _alpha;

        if (_bossGone && _alpha <= 0f) Destroy(gameObject);
    }

    private void ApplyFill()
    {
        SetWidth(_fill, _fillAmount);
        SetWidth(_fillLight, _fillAmount);
        SetWidth(_damageTrail, _trailAmount);
    }

    private static void SetWidth(RectTransform rect, float amount)
    {
        var max = rect.anchorMax;
        max.x = amount;
        rect.anchorMax = max;
    }

    private void Build(string bossName, TMP_FontAsset font)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0f;

        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        var root = (RectTransform)transform;

        var nameRect = CreateRect("Name", root);
        AnchorTop(nameRect, NameTopOffset, BarWidth, NameFontSize + 8f);
        var nameText = nameRect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font) nameText.font = font;
        nameText.text = bossName;
        nameText.fontSize = NameFontSize;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.raycastTarget = false;

        var frame = CreateRect("Frame", root);
        AnchorTop(frame, BarTopOffset - BorderSize, BarWidth + BorderSize * 2f, BarHeight + BorderSize * 2f);
        AddImage(frame, PanelColor);

        var background = CreateRect("Background", frame);
        Stretch(background, BorderSize);
        AddImage(background, BackgroundColor);

        _damageTrail = CreateFillLayer("DamageTrail", background, DamageTrailColor, 0f);
        _fill = CreateFillLayer("Fill", background, FillColor, 0f);
        _fillLight = CreateFillLayer("FillLight", background, FillLightColor, LightFillBottom);
    }

    private static RectTransform CreateFillLayer(string name, RectTransform parent, Color color, float bottom)
    {
        var rect = CreateRect(name, parent);
        rect.anchorMin = new Vector2(0f, bottom);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        AddImage(rect, color);
        return rect;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static void AnchorTop(RectTransform rect, float topOffset, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -topOffset);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static void AddImage(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }
}
