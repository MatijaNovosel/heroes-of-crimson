using System.Collections;
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

    [Header("Spawn Bounce")]
    public float BounceDuration = 0.3f;
    public float BounceScale = 1.2f;
    public float BounceHeight = 0.25f;
    public bool shouldBounce = false;

    private bool isUIActive;
    private RectTransform _inventoryUIRect;
    private SpriteRenderer _spriteRenderer;
    private readonly List<Item> _seededItems = new();
    private bool _lootGenerated;
    private Vector3 _originalScale;

    public int[] initialItemIds;

    private void Start()
    {
        _originalScale = transform.localScale;

        _player = GameObject.Find("Player");
        _lootBagUI = GameObject.Find("LootContainerGroup");
        _lootContainerInventory = GameObject.Find("LootContainerInventory");
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_lootBagUI != null) _inventoryUIRect = _lootBagUI.GetComponent<RectTransform>();
        if (initialItemIds != null && initialItemIds.Length > 0) GenerateLoot(initialItemIds);

        if (shouldBounce)
        {
            StartCoroutine(BounceIn());
        }
    }

    private void Update()
    {
        if (_player == null || _lootContainerInventory == null) return;

        float distance = Vector3.Distance(_player.transform.position, transform.position);
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

    private IEnumerator BounceIn()
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        transform.localScale = Vector3.zero;

        while (elapsed < BounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / BounceDuration);

            float scale = t < 0.6f
                ? Mathf.Lerp(0f, BounceScale, t / 0.6f)
                : Mathf.Lerp(BounceScale, 1f, (t - 0.6f) / 0.4f);

            transform.localScale = _originalScale * scale;
            transform.position = startPosition + Vector3.up * (Mathf.Sin(t * Mathf.PI) * BounceHeight);

            yield return null;
        }

        transform.localScale = _originalScale;
        transform.position = startPosition;
    }

    public void GenerateLoot(int[] itemIds)
    {
        if (_lootGenerated || itemIds == null || itemIds.Length == 0) return;

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
        if (_lootGenerated || lootTable.Items == null || lootTable.Items.Count == 0) return;

        _lootGenerated = true;

        int[] rolledIds = LootRoller.Roll(lootTable, randomItemCount);
        Debug.Log($"Loot table {lootTable.Id} rolled {rolledIds.Length} items.");

        foreach (int itemId in rolledIds)
        {
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
        if (item != null) _seededItems.Add(item);
    }

    public List<Item> GetLootItems() => _seededItems;

    public void TryDestroyIfEmpty()
    {
        if (_seededItems.Count > 0 || !DestroyIfNoItems) return;

        if (_inventoryUIRect != null) _inventoryUIRect.localScale = Vector3.zero;

        isUIActive = false;
        Destroy(gameObject);
    }

    public void RemoveItem(Item item)
    {
        if (item != null) _seededItems.Remove(item);
    }
}