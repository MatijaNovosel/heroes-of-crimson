using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    private GameObject statusEffectIconPrefab;
    public List<StatusEffectUIModel> statusEffects;
    public List<Constants.StatusEffects> values;
    
    void Start()
    {
        statusEffectIconPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectIcon");
    }

    void Update()
    {
        foreach (var statusEffect in values)
        {
            if (statusEffects.Any(x => x.StatusEffect == statusEffect))
            {
                return;
            }
            else
            {
                var statusEffectIcon = Instantiate(
                    statusEffectIconPrefab,
                    new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0),
                    Quaternion.identity
                );
                statusEffects.Add(new StatusEffectUIModel(statusEffectIcon, statusEffect));
            }
        }
    }
}
