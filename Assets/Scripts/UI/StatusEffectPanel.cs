using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    private GameObject statusEffectIconPrefab;
    public List<StatusEffectUIModel> statusEffects;
    public HashSet<Constants.StatusEffects> values;
    public GameObject Obj;
    
    void Start()
    {
        statusEffectIconPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectIcon");
    }

    public void SetStatusEffects(List<Constants.StatusEffects> statusEffects)
    {
        statusEffects.AddRange(statusEffects);
    }

    public void Setup(List<Constants.StatusEffects> statusEffects, GameObject obj)
    {
        statusEffects.AddRange(statusEffects);
        Obj = obj;
    }

    void Update()
    {
        transform.position = new Vector3(Obj.transform.position.x, Obj.transform.position.y, 0);
        
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
                    new Vector3(transform.position.x, transform.position.y, 0),
                    Quaternion.identity
                );
                statusEffects.Add(new StatusEffectUIModel(statusEffectIcon, statusEffect));
            }
        }
    }
}
