using UI.Inventory;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    private GameObject _lootBagUI;
    private GameObject _player;
    public float InteractionRange = 3f;
    private bool isUIActive = false;
    
    void Start()
    {
        _player = GameObject.Find("Player");
        _lootBagUI = GameObject.Find("LootBagGroup");
    }

    void Update()
    {
        if (_player is null) return;

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