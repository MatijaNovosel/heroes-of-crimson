using System;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
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

    public void SetInfo(
        string title,
        string description,
        Constants.ItemTag tag,
        Constants.ItemRarity rarity,
        int minDamage,
        int maxDamage,
        List<int> stats
    )
    {
        TitleText.text = title;
        DescriptionText.text = description;
        TypeText.text = tag.ToString();
        
        TooltipDamage.text = $"Damage: {minDamage} - {maxDamage}";

        if (tag == Constants.ItemTag.Weapon)
        {
            TooltipDamage.gameObject.SetActive(true);
            DividerWeapons.gameObject.SetActive(true);
        }
        else
        {
            TooltipDamage.gameObject.SetActive(false);
            DividerWeapons.gameObject.SetActive(false);
        }

        if (!stats.TrueForAll(x => x == 0))
        {
            DividerStats.gameObject.SetActive(true);
            TooltipStats.gameObject.SetActive(true);
            
            if (stats[(int)Constants.Stats.MGT] != 0)
            {
                TooltipMgt.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.MGT].ToString();
                TooltipMgt.gameObject.SetActive(true);
            }
            else
            {
                TooltipMgt.gameObject.SetActive(false);
            }
            
            if (stats[(int)Constants.Stats.ARM] != 0)
            {
                TooltipArm.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.ARM].ToString();
                TooltipArm.gameObject.SetActive(true);
            }
            else
            {
                TooltipArm.gameObject.SetActive(false);
            }
            
            if (stats[(int)Constants.Stats.WIS] != 0)
            {
                TooltipWis.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.WIS].ToString();
                TooltipWis.gameObject.SetActive(true);
            }
            else
            {
                TooltipWis.gameObject.SetActive(false);
            }
            
            if (stats[(int)Constants.Stats.STR] != 0)
            {
                TooltipStr.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.STR].ToString();
                TooltipStr.gameObject.SetActive(true);
            }
            else
            {
                TooltipStr.gameObject.SetActive(false);
            }
            
            if (stats[(int)Constants.Stats.AGI] != 0)
            {
                TooltipAgi.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.AGI].ToString();
                TooltipAgi.gameObject.SetActive(true);
            }
            else
            {
                TooltipAgi.gameObject.SetActive(false);
            }
            
            if (stats[(int)Constants.Stats.SWF] != 0)
            {
                TooltipSwf.GetComponentInChildren<TMP_Text>().text = stats[(int)Constants.Stats.SWF].ToString();
                TooltipSwf.gameObject.SetActive(true);
            }
            else
            {
                TooltipSwf.gameObject.SetActive(false);
            }
        }
        else
        {
            DividerStats.gameObject.SetActive(false);
            TooltipStats.gameObject.SetActive(false);
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
