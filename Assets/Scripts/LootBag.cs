using HeroesOfCrimson.Utils;
using UI.Inventory;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    private GameObject _lootBagUI;
    private GameObject _player;
    public float InteractionRange = 1f;
    private bool isUIActive = false;
    
    private LineRenderer _rangeCircle;
    
    void Start()
    {
        _player = GameObject.Find("Player");
        _lootBagUI = GameObject.Find("LootBagGroup");
        
        _rangeCircle = Utils.CreateCircle(
            transform,
            "AttackRange",
            InteractionRange,
            new Color(0.8f, 0f, 0f, 0.4f)
        );
    }

    void Update()
    {
        if (!_player) return;

        float distance = Vector3.Distance(_player.transform.position, transform.position);
        bool isNear = distance <= InteractionRange;
        RectTransform inventoryUIRect = _lootBagUI.GetComponent<RectTransform>();
        
        if (isNear && !isUIActive)
        {
            inventoryUIRect.localScale = Vector3.one;
            isUIActive = true;
        }
        else if (!isNear && isUIActive)
        {
            inventoryUIRect.localScale = Vector3.zero;
            isUIActive = false;
        }
    }
}