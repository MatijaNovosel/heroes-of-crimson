using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectBarItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;
    public Image image;
    public Constants.StatusEffects statusEffect;
    public StatusEffectData data;

    private bool _isPointerOver;

    public void Initialize(ActiveStatusEffect effect)
    {
        statusEffect = effect.Type;
        data = Utils.GetStatusEffectData(effect.Type);

        var sprite = ResourceCacher.Singleton.StatusEffectSprites[(int)statusEffect - 1];
        image.sprite = sprite;

        background.color = data.IsNegative
            ? Utils.FromHex("A43737")
            : Utils.FromHex("4E6C46");

        UpdateFill(1f);
    }

    public void UpdateFill(float normalized)
    {
        background.fillAmount = normalized;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        StatusEffectTooltipManager.Singleton.SetInfo(data);
        StatusEffectTooltipManager.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        StatusEffectTooltipManager.Singleton.Hide();
    }

    private void OnDisable()
    {
        if (_isPointerOver)
        {
            StatusEffectTooltipManager.Singleton.Hide();
            _isPointerOver = false;
        }
    }

    private void OnDestroy()
    {
        if (_isPointerOver && StatusEffectTooltipManager.Singleton != null)
        {
            StatusEffectTooltipManager.Singleton.Hide();
        }
    }
}
