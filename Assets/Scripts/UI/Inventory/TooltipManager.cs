using System;
using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public Canvas ParentCanvas;
    public Transform TooltipTransform;
    public static TooltipManager Singleton;
    public TMP_Text TitleText;
    public TMP_Text DescriptionText;
    public TMP_Text TypeText;
    public Image TooltipRarityImg;
    
    void Start()
    {
        Singleton = this;
    }

    public void SetInfo(string title, string description, Constants.SlotTag tag, Constants.ItemRarity rarity)
    {
        TitleText.text = title;
        DescriptionText.text = description;
        TypeText.text = tag.ToString();
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
        TooltipTransform.position = ParentCanvas.transform.TransformPoint(mousePosition);
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
