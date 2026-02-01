using System;
using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class StatusEffectBar : MonoBehaviour
{
    public Player player;
    private BaseNPCBehaviour _baseNpcBehaviour;
    public GameObject statusEffectBarItemPrefab;

    private Dictionary<Constants.StatusEffects, StatusEffectBarItem> _items = new();

    private void Awake()
    {
        _baseNpcBehaviour = player.GetComponent<BaseNPCBehaviour>();
    }

    void Update()
    {
        if (_baseNpcBehaviour == null) return;

        var activeEffects = _baseNpcBehaviour.ActiveStatusEffects;

        foreach (var effect in activeEffects)
        {
            if (_items.ContainsKey(effect.Type)) continue;

            var go = Instantiate(statusEffectBarItemPrefab, transform);
            var item = go.GetComponent<StatusEffectBarItem>();
            item.Initialize(effect);
            _items.Add(effect.Type, item);
        }

        foreach (var effect in activeEffects)
        {
            if (_items.TryGetValue(effect.Type, out var item))
            {
                item.UpdateFill(effect.NormalizedTime);
            }
        }

        var activeTypes = activeEffects.Select(e => e.Type).ToHashSet();

        var toRemove = _items.Keys.Where(t => !activeTypes.Contains(t)).ToList();
        foreach (var type in toRemove)
        {
            Destroy(_items[type].gameObject);
            _items.Remove(type);
        }
    }
}