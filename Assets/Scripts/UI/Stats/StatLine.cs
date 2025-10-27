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
    private BaseNPCBehaviour _playerBaseNpcBehaviour;
    
    void Start()
    {
        _statText = statLineText.GetComponent<TMP_Text>();
        _playerBaseNpcBehaviour = player.GetComponent<BaseNPCBehaviour>();
    }

    void Update()
    {
        _statText.text = stat switch
        {
            Constants.Stats.ATT => _playerBaseNpcBehaviour.att.ToString(),
            Constants.Stats.DEF => _playerBaseNpcBehaviour.def.ToString(),
            Constants.Stats.WIS => _playerBaseNpcBehaviour.wis.ToString(),
            Constants.Stats.VIT => _playerBaseNpcBehaviour.vit.ToString(),
            Constants.Stats.DEX => _playerBaseNpcBehaviour.dex.ToString(),
            Constants.Stats.SPD => _playerBaseNpcBehaviour.spd.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
