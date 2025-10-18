using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    private GameObject StatusEffectIconPrefab;
    public List<StatusEffectUIModel> statusEffects = new();
    private readonly HashSet<Constants.StatusEffects> values = new();
    public GameObject Obj;
    
    void Start()
    {
        StatusEffectIconPrefab ??= Resources.Load<GameObject>("Prefabs/StatusEffectIcon");
    }

    public void SetStatusEffects(List<Constants.StatusEffects> newStatusEffects)
    {
        foreach (var effect in newStatusEffects)
            values.Add(effect);
    }

    public void Setup(List<Constants.StatusEffects> setupValues, GameObject obj)
    {
        foreach (var statusEffect in setupValues)
            values.Add(statusEffect);

        Obj = obj;
    }

    void Update()
    {
        if (Obj is null) return;
        print(values.Count);

        transform.position = new Vector3(Obj.transform.position.x, Obj.transform.position.y + 1.15f, 0);

        foreach (var statusEffect in values)
        {
            var statusObject = statusEffects.FirstOrDefault(x => x.StatusEffect == statusEffect);

            if (EqualityComparer<StatusEffectUIModel>.Default.Equals(statusObject, default))
            {
                var statusEffectIcon = Instantiate(
                    StatusEffectIconPrefab,
                    transform.position,
                    Quaternion.identity
                );
                statusEffects.Add(new StatusEffectUIModel(statusEffectIcon, statusEffect));
            }
            else
            {
                statusObject.Icon.transform.position = transform.position;
            }
        }
    }
}