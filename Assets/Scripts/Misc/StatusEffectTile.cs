using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StatusEffectTile : MonoBehaviour
{
    public Constants.StatusEffects statusEffect;
    public float duration = 2f;
    public float tickInterval = 1f;

    private readonly Dictionary<BaseNPCBehaviour, float> _nextTickAt = new();

    private void OnTriggerStay2D(Collider2D other)
    {
        var npc = other.GetComponent<BaseNPCBehaviour>();
        if (!npc) return;

        float now = Time.time;
        if (_nextTickAt.TryGetValue(npc, out var next) && now < next) return;

        _nextTickAt[npc] = now + tickInterval;
        npc.ApplyStatusEffect(statusEffect, duration);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var npc = other.GetComponent<BaseNPCBehaviour>();
        if (!npc) return;
        _nextTickAt.Remove(npc);
    }
}