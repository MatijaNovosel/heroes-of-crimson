using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;

public class StatTooltipManager : MonoBehaviour
{
    public static StatTooltipManager Singleton;
    public Canvas ParentCanvas;
    
    public RectTransform TooltipTransform;
    public RectTransform TooltipBg;
    
    public TMP_Text TitleText;
    public TMP_Text DescriptionText;
    
    void Start()
    {
        Singleton = this;
    }

    public void SetInfo(string name, string description, string color)
    {
        TitleText.text = name;
        TitleText.color = Utils.FromHex(color);
        DescriptionText.text = description;
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
