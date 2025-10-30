using System;
using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public Canvas ParentCanvas;
    public RectTransform TooltipTransform;
    public RectTransform TooltipBg;
    public static TooltipManager Singleton;
    public TMP_Text TitleText;
    public TMP_Text DescriptionText;
    public TMP_Text TypeText;
    public TMP_Text TooltipDamageMin;
    public TMP_Text TooltipDamageMax;
    public Divider DividerWeapons;
    public Image TooltipRarityImg;
    
    void Start()
    {
        Singleton = this;
    }

    public void SetInfo(
        string title,
        string description,
        Constants.SlotTag tag,
        Constants.ItemRarity rarity,
        int minDamage,
        int maxDamage
    )
    {
        TitleText.text = title;
        DescriptionText.text = description;
        TypeText.text = tag.ToString();
        
        TooltipDamageMax.text = $"Damage (min): {minDamage}";
        TooltipDamageMin.text = $"Damage (max): {maxDamage}";

        if (tag == Constants.SlotTag.Weapon)
        {
            TooltipDamageMin.enabled = true;
            TooltipDamageMax.enabled = true;
            DividerWeapons.gameObject.SetActive(true);
        }
        else
        {
            TooltipDamageMin.enabled = false;
            TooltipDamageMax.enabled = false;
            DividerWeapons.gameObject.SetActive(false);
        }
            
        TooltipRarityImg.color = rarity switch
        {
            Constants.ItemRarity.Common => Color.white,
            Constants.ItemRarity.Uncommon => Color.limeGreen,
            Constants.ItemRarity.Rare => Color.dodgerBlue,
            Constants.ItemRarity.Epic => Color.purple,
            Constants.ItemRarity.Legendary => Color.darkOrange,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
        };
    }

    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ParentCanvas.transform as RectTransform,
            Input.mousePosition,
            ParentCanvas.worldCamera,
            out var mousePosition
        );

        var tooltipHeight = TooltipBg.rect.height;
        var offset = new Vector2(0, tooltipHeight / 2);
        var transformPoint = ParentCanvas.transform.TransformPoint(mousePosition + offset);
        
        TooltipTransform.position = transformPoint;
    }


    public void Show()
    {
        TooltipTransform.gameObject.SetActive(true);
    }

    public void Hide()
    {
        TooltipTransform.gameObject.SetActive(false);
    }
}
