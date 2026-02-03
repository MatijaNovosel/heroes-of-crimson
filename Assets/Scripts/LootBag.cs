using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using UI.Inventory;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    private GameObject _lootBagUI;
    private GameObject _lootContainerInventory;
    private GameObject _player;
    public float InteractionRange = 1f;
    private bool isUIActive = false;

    private LineRenderer _rangeCircle;
    private RectTransform _inventoryUIRect;
    
    private List<Item> _seededItems = new List<Item>();
    private bool _lootGenerated = false;
    public int[] initialItemIds;

    void Start()
    {
        _player = GameObject.Find("Player");
        _lootBagUI = GameObject.Find("LootContainerGroup");
        _lootContainerInventory = GameObject.Find("LootContainerInventory");

        _rangeCircle = Utils.CreateCircle(
            transform,
            "InteractionRange",
            InteractionRange,
            new Color(0.8f, 0f, 0f, 0.4f)
        );

        _inventoryUIRect = _lootBagUI.GetComponent<RectTransform>();

        if (initialItemIds.Length > 0)
        {
            GenerateLoot(initialItemIds);
        }
    }

    void Update()
    {
        if (!_player) return;

        float distance = Vector3.Distance(_player.transform.position, transform.position);
        bool isNear = distance <= InteractionRange;
        var lootInventory = _lootContainerInventory.GetComponent<Inventory>();

        if (isNear && !isUIActive)
        {
            _inventoryUIRect.localScale = Vector3.one;
            isUIActive = true;
            lootInventory.ShowLoot(this);
        }
        else if (!isNear && isUIActive)
        {
            _inventoryUIRect.localScale = Vector3.zero;
            isUIActive = false;
        }
    }

    public void GenerateLoot(int[] itemIds, bool? randomize = false)
    {
        if (_lootGenerated || itemIds.Length == 0) return;
        _lootGenerated = true;

        if (randomize == true)
        {
            int seed = GetInstanceID();
            Random.InitState(seed);
            int itemCount = Random.Range(1, 4);

            for (int i = 0; i < itemCount; i++)
            {
                int id = itemIds[Random.Range(0, itemIds.Length)];
                _seededItems.Add(Database.Singleton.GetItem(id));
            }   
        }
        else
        {
            foreach (var id in itemIds)
            {
                _seededItems.Add(Database.Singleton.GetItem(id));
            }
        }
    }
    
    public void AddItem(Item item)
    {
        _seededItems.Add(item);
    }

    public List<Item> GetLootItems() => _seededItems;
    
    public void TryDestroyIfEmpty()
    {
        if (_seededItems.Count > 0) return;

        _inventoryUIRect.localScale = Vector3.zero;
        isUIActive = false;
        Destroy(gameObject);
    }
    
    public void RemoveItem(Item item)
    {
        _seededItems.Remove(item);
    }
}
