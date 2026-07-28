using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalentTreeItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Image background;

    public int TalentIdValue => _talent != null ? _talent.Id : -1;
    public RectTransform RectTransform { get; private set; }

    private TalentModel _talent;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Init(TalentModel talent)
    {
        _talent = talent;

        if (image != null)
        {
            image.sprite = talent.Sprite;
            image.enabled = talent.Sprite != null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LearnTalent();
    }

    private void LearnTalent()
    {
        if (_talent == null) return;
        if (Player.Singleton.learnedTalents.Contains(_talent.Id)) return;
        Player.Singleton.LearnTalent(_talent.Id);
    }

    private void Update()
    {
        if (_talent == null || image == null || background == null) return;
        bool learned = Player.Singleton.learnedTalents.Contains(_talent.Id);
        image.color = learned ? Color.white : Utils.FromHex("504141");
        background.color = learned ? Utils.FromHex("632C2C") : Utils.FromHex("0C0808");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_talent == null) return;
        TalentTreeItemTooltipManager.Singleton.SetInfo(_talent.Name, _talent.Description);
        TalentTreeItemTooltipManager.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TalentTreeItemTooltipManager.Singleton.Hide();
    }
}