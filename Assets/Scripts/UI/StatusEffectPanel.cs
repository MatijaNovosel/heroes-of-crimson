using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    private GameObject statusEffectIconPrefab;
    private float iconSpacing = 0.6f;

    private readonly List<StatusEffectUIModel> activeIcons = new();
    private Sprite[] sprites;
    private HashSet<Constants.StatusEffects> currentEffects = new();

    private GameObject Obj;

    void Awake()
    {
        statusEffectIconPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectIcon");
        sprites = Resources.LoadAll<Sprite>("Sprites/statusEffects");
    }

    public void Setup(List<Constants.StatusEffects> setupValues, GameObject obj)
    {
        currentEffects = new HashSet<Constants.StatusEffects>(setupValues);
        Obj = obj;
        RefreshIcons();
    }

    public void SetStatusEffects(List<Constants.StatusEffects> newStatusEffects)
    {
        currentEffects = new HashSet<Constants.StatusEffects>(newStatusEffects);
        RefreshIcons();
    }

    private void RefreshIcons()
    {
        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            if (currentEffects.Contains(activeIcons[i].StatusEffect)) continue;
            Destroy(activeIcons[i].Icon);
            activeIcons.RemoveAt(i);
        }

        foreach (var effect in currentEffects)
        {
            if (activeIcons.Any(x => x.StatusEffect == effect)) continue;
            var icon = Instantiate(statusEffectIconPrefab, transform);
            icon.GetComponent<SpriteRenderer>().sprite = sprites[(int)effect - 1];
            activeIcons.Add(new StatusEffectUIModel(icon, effect));
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
        var totalWidth = (activeIcons.Count - 1) * iconSpacing;
        var startOffset = -totalWidth / 2f;

        for (int i = 0; i < activeIcons.Count; i++)
        {
            var offsetX = startOffset + i * iconSpacing;
            var iconObj = activeIcons[i].Icon;
            if (iconObj is null) continue;

            iconObj.transform.position = new Vector3(
                transform.position.x + offsetX,
                transform.position.y,
                transform.position.z
            );
        }
    }
}
