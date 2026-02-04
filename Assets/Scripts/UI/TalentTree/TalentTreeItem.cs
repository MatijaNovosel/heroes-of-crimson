using System;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalentTreeItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Constants.Talents talentId;
    public Image image;
    public Image background;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _learnTalent();
    }

    private void _learnTalent()
    {
        if (Player.Singleton.learnedTalents.Contains((int)talentId)) return;
        Player.Singleton.LearnTalent(talentId);
    }

    private void Update()
    {
        bool learned = Player.Singleton.learnedTalents.Contains((int)talentId);
        image.color = learned ? Color.white : Utils.FromHex("504141");
        background.color = learned ? Utils.FromHex("632C2C") : Utils.FromHex("0C0808");
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var data = Utils.GetTalentTreeItemData(talentId);
        TalentTreeItemTooltipManager.Singleton.SetInfo(data.Name, data.Description);
        TalentTreeItemTooltipManager.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TalentTreeItemTooltipManager.Singleton.Hide();
    }
}
