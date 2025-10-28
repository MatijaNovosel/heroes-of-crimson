using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = HeroesOfCrimson.Utils.Utils;

public class TooltipManager : MonoBehaviour
{
    public Canvas ParentCanvas;
    public Transform TooltipTransform;
    public static TooltipManager Singleton;
    public TMP_Text TitleText;
    public TMP_Text DescriptionText;
    
    void Start()
    {
        Singleton = this;
    }

    public void SetInfo(string title, string description)
    {
        TitleText.text = title;
        DescriptionText.text = description;
    }

    void Update()
    {
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentCanvas.transform as RectTransform, Input.mousePosition, ParentCanvas.worldCamera, out mousePosition);
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
