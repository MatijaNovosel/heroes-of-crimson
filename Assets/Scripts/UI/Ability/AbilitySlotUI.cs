using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string name;
    public string description;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        AbilityTooltipManager.Singleton.SetInfo(name, description);
        AbilityTooltipManager.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AbilityTooltipManager.Singleton.Hide();
    }
}
