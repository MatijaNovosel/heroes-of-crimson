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
    public bool DestroyIfNoItems = true;

    private bool isUIActive = false;

    private RectTransform _inventoryUIRect;
    private SpriteRenderer _spriteRenderer;

    private readonly List<Item> _seededItems = new List<Item>();

    private bool _lootGenerated = false;

    public int[] initialItemIds;

    private void Start()
    {
        _player = GameObject.Find("Player");
        _lootBagUI = GameObject.Find("LootContainerGroup");
        _lootContainerInventory = GameObject.Find("LootContainerInventory");

        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_lootBagUI != null)
        {
            _inventoryUIRect = _lootBagUI.GetComponent<RectTransform>();
        }

        if (initialItemIds != null && initialItemIds.Length > 0)
        {
            GenerateLoot(initialItemIds);
        }
    }

    private void Update()
    {
        if (_player == null) return;
        if (_lootContainerInventory == null) return;

        float distance = Vector3.Distance(
            _player.transform.position,
            transform.position
        );

        bool isNear = distance <= InteractionRange;
        Inventory lootInventory = _lootContainerInventory.GetComponent<Inventory>();

        if (isNear && !isUIActive)
        {
            if (_inventoryUIRect != null) _inventoryUIRect.localScale = Vector3.one;
            isUIActive = true;
            lootInventory.ShowLoot(this);
            if (_spriteRenderer != null) _spriteRenderer.color = Color.red;
        }
        else if (!isNear && isUIActive)
        {
            if (_inventoryUIRect != null) _inventoryUIRect.localScale = Vector3.zero;
            isUIActive = false;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
        }
    }

    public void GenerateLoot(int[] itemIds)
    {
        if (_lootGenerated) return;
        if (itemIds == null || itemIds.Length == 0) return;
        _lootGenerated = true;

        foreach (int id in itemIds)
        {
            Item item = Database.Singleton.GetItem(id);
            if (item == null)
            {
                Debug.LogError($"LootBag could not find item with ID {id}.");
                continue;
            }
            _seededItems.Add(item);
        }
    }

    public void GenerateLoot(LootTableModel lootTable, int randomItemCount = 0)
    {
        if (_lootGenerated) return;
        if (lootTable.Items == null || lootTable.Items.Count == 0) return;
        _lootGenerated = true;
        int[] rolledIds = LootRoller.Roll(lootTable, randomItemCount);

        Debug.Log($"Loot table {lootTable.Id} rolled {rolledIds.Length} items.");

        for (int i = 0; i < rolledIds.Length; i++)
        {
            int itemId = rolledIds[i];
            Item item = Database.Singleton.GetItem(itemId);

            if (item == null)
            {
                Debug.LogError($"LootBag could not find item with ID {itemId}.");
                continue;
            }
            Debug.Log($"Loot: {itemId} -> {item.name}");
            _seededItems.Add(item);
        }
    }

    public void AddItem(Item item)
    {
        if (item == null) return;
        _seededItems.Add(item);
    }

    public List<Item> GetLootItems()
    {
        return _seededItems;
    }

    public void TryDestroyIfEmpty()
    {
        if (_seededItems.Count > 0) return;
        if (!DestroyIfNoItems) return;
        if (_inventoryUIRect != null) _inventoryUIRect.localScale = Vector3.zero;
        isUIActive = false;
        Destroy(gameObject);
    }

    public void RemoveItem(Item item)
    {
        if (item == null) return;
        _seededItems.Remove(item);
    }
}