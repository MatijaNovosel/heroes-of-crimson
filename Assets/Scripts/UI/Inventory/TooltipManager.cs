using System;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Singleton;
    
    public Canvas ParentCanvas;
    
    // Root elements
    public RectTransform TooltipTransform;
    public RectTransform TooltipBg;
    
    // Title
    public TMP_Text TitleText;
    public TMP_Text TypeText;
    public Image TooltipRarityImg;
    
    // Description
    public TMP_Text DescriptionText;
    
    // Damage
    public TMP_Text TooltipDamage;
    public Divider DividerWeapons;
    
    // Range
    public TMP_Text TooltipRange;
    
    // Stats
    public Divider DividerStats;
    public RectTransform TooltipStats;
    public Image TooltipMgt;
    public Image TooltipSwf;
    public Image TooltipAgi;
    public Image TooltipArm;
    public Image TooltipStr;
    public Image TooltipWis;
    
    void Start()
    {
        Singleton = this;
    }
    
    private void SetActive(GameObject go, bool state)
    {
        if (go.activeSelf != state) go.SetActive(state);
    }

    private void SetStat(Image image, int value)
    {
        bool active = value != 0;
        SetActive(image.gameObject, active);
        if (active) image.GetComponentInChildren<TMP_Text>().text = value.ToString();
    }

    public void SetInfo(Item item)
    {
        TitleText.text = item.name;
        DescriptionText.text = item.description;
        TypeText.text = item.tag.ToString();

        bool isWeapon = item.tag == Constants.ItemTag.Weapon;

        SetActive(TooltipDamage.gameObject, isWeapon);
        SetActive(TooltipRange.gameObject, isWeapon);
        SetActive(DividerWeapons.gameObject, isWeapon);

        if (isWeapon)
        {
            TooltipDamage.text = $"Damage: {item.minDamage} - {item.maxDamage}";
            TooltipRange.text = $"Range: {item.range}";
        }

        bool hasAnyStats = item.stats.Exists(x => x != 0);

        SetActive(DividerStats.gameObject, hasAnyStats);
        SetActive(TooltipStats.gameObject, hasAnyStats);

        if (hasAnyStats)
        {
            SetStat(TooltipMgt, item.stats[(int)Constants.Stats.MGT]);
            SetStat(TooltipArm, item.stats[(int)Constants.Stats.ARM]);
            SetStat(TooltipWis, item.stats[(int)Constants.Stats.WIS]);
            SetStat(TooltipStr, item.stats[(int)Constants.Stats.STR]);
            SetStat(TooltipAgi, item.stats[(int)Constants.Stats.AGI]);
            SetStat(TooltipSwf, item.stats[(int)Constants.Stats.SWF]);
        }

        TooltipRarityImg.color = item.rarity switch
        {
            Constants.ItemRarity.Common    => Color.white,
            Constants.ItemRarity.Uncommon  => Color.limeGreen,
            Constants.ItemRarity.Rare      => Color.dodgerBlue,
            Constants.ItemRarity.Epic      => Color.purple,
            Constants.ItemRarity.Legendary => Color.darkOrange,
            _ => Color.white
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

        var tooltipWidth = TooltipBg.rect.width;
        var tooltipHeight = TooltipBg.rect.height;
        const float padding = 3f;

        var offset = new Vector2(padding, tooltipHeight / 2 + padding);

        var worldPoint = ParentCanvas.transform.TransformPoint(mousePosition + offset);
        TooltipTransform.position = worldPoint;

        var corners = new Vector3[4];
        TooltipBg.GetWorldCorners(corners);

        var outOfBoundsTop = corners[1].y > Screen.height;
        var outOfBoundsRight = corners[2].x > Screen.width;

        if (outOfBoundsTop) offset.y = -(tooltipHeight / 2 + padding);
        if (outOfBoundsRight) offset.x = -(tooltipWidth + padding);

        worldPoint = ParentCanvas.transform.TransformPoint(mousePosition + offset);
        TooltipTransform.position = worldPoint;
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
