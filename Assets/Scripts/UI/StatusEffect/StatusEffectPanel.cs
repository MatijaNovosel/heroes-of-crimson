using System.Collections.Generic;
using System.Linq;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    private GameObject _statusEffectIconPrefab;
    private readonly float _iconSpacing = 0.6f;

    private readonly List<StatusEffectUIModel> _activeIcons = new();
    private HashSet<Constants.StatusEffects> _currentEffects = new();

    private GameObject Obj;

    void Awake()
    {
        _statusEffectIconPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectIcon");
    }

    public void Setup(List<Constants.StatusEffects> setupValues, GameObject obj)
    {
        _currentEffects = new HashSet<Constants.StatusEffects>(setupValues);
        Obj = obj;
        RefreshIcons();
    }

    public void SetStatusEffects(List<Constants.StatusEffects> newStatusEffects)
    {
        _currentEffects = new HashSet<Constants.StatusEffects>(newStatusEffects);
        RefreshIcons();
    }

    private void RefreshIcons()
    {
        for (int i = _activeIcons.Count - 1; i >= 0; i--)
        {
            if (_currentEffects.Contains(_activeIcons[i].StatusEffect)) continue;
            Destroy(_activeIcons[i].Icon);
            _activeIcons.RemoveAt(i);
        }

        foreach (var effect in _currentEffects)
        {
            if (_activeIcons.Any(x => x.StatusEffect == effect)) continue;
            var icon = Instantiate(_statusEffectIconPrefab, transform);
            icon.GetComponent<SpriteRenderer>().sprite = ResourceCacher.Singleton.StatusEffectSprites[(int)effect - 1];
            _activeIcons.Add(new StatusEffectUIModel(icon, effect));
        }

        RepositionIcons();
    }

    private void Update()
    {
        if (!Obj)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = new Vector3(Obj.transform.position.x, Obj.transform.position.y + 1f, 0);
        RepositionIcons();
    }

    private void RepositionIcons()
    {
        var totalWidth = (_activeIcons.Count - 1) * _iconSpacing;
        var startOffset = -totalWidth / 2f;

        for (int i = 0; i < _activeIcons.Count; i++)
        {
            var offsetX = startOffset + i * _iconSpacing;
            var iconObj = _activeIcons[i].Icon;
            if (iconObj is null) continue;

            iconObj.transform.position = new Vector3(
                transform.position.x + offsetX,
                transform.position.y,
                transform.position.z
            );
        }
    }
}
