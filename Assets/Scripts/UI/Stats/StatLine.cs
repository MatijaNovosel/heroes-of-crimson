using System;
using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatLine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject statLineText;
    public Player player;
    public Constants.Stats stat;

    private TMP_Text _statText;
    
    private void Start()
    {
        _statText = statLineText.GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _statText.text = stat switch
        {
            Constants.Stats.MGT => player.actualMgt.ToString(),
            Constants.Stats.ARM => player.actualArm.ToString(),
            Constants.Stats.WIS => player.actualWis.ToString(),
            Constants.Stats.STR => player.actualStr.ToString(),
            Constants.Stats.AGI => player.actualAgi.ToString(),
            Constants.Stats.SWF => player.actualSwf.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var data = Utils.GetStatData(stat);
        StatTooltipManager.Singleton.SetInfo(data.Name, data.Description, data.Color);
        StatTooltipManager.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatTooltipManager.Singleton.Hide();
    }
}
