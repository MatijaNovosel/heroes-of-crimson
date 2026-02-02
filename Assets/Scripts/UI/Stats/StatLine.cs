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
            Constants.Stats.MGT => player.actualMgt.ToString(),
            Constants.Stats.ARM => player.actualArm.ToString(),
            Constants.Stats.WIS => player.actualWis.ToString(),
            Constants.Stats.STR => player.actualStr.ToString(),
            Constants.Stats.AGI => player.actualAgi.ToString(),
            Constants.Stats.SWF => player.actualSwf.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
