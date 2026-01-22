using System;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AbilitySlot
{
    public KeyCode key;
    public Image cooldownImage;
    public float cooldown = 1f;
    public float manaCost = 30f;
    public bool ignoreForbiddenTiles;
    public Constants.AbilityType abilityType;

    [HideInInspector] public float lastUsedTime = -999f;

    public bool IsReady => Time.time - lastUsedTime >= cooldown;

    public float CooldownRemaining =>
        Mathf.Max(0, cooldown - (Time.time - lastUsedTime));
}