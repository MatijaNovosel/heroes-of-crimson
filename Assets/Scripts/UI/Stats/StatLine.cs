using System;
using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;

public class StatLine : MonoBehaviour
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
            Constants.Stats.ATT => player.actualAtt.ToString(),
            Constants.Stats.DEF => player.actualDef.ToString(),
            Constants.Stats.WIS => player.actualWis.ToString(),
            Constants.Stats.VIT => player.actualVit.ToString(),
            Constants.Stats.DEX => player.actualDex.ToString(),
            Constants.Stats.SPD => player.actualSpd.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
