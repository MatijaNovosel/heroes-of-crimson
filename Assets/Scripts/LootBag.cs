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
    private static readonly HashSet<LootBag> ActiveLootBags = new();

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
    
    private void OnEnable()
    {
        ActiveLootBags.Add(this);
    }

    private void OnDisable()
    {
        ActiveLootBags.Remove(this);
    }
    
    public static LootBag GetNearest(Vector3 position, float maxDistance)
    {
        LootBag nearest = null;
        float nearestDistanceSqr = maxDistance * maxDistance;

        foreach (LootBag bag in ActiveLootBags)
        {
            if (!bag) continue;
            float distanceSqr = ((Vector2)bag.transform.position - (Vector2)position).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr) continue;
            nearest = bag;
            nearestDistanceSqr = distanceSqr;
        }

        return nearest;
    }

    private static LootBag GetNearestInInteractionRange(Vector3 position, LootBag excluded = null)
    {
        LootBag nearest = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (LootBag bag in ActiveLootBags)
        {
            if (!bag || bag == excluded) continue;

            float distanceSqr = ((Vector2)bag.transform.position - (Vector2)position).sqrMagnitude;
            float rangeSqr = bag.InteractionRange * bag.InteractionRange;
            if (distanceSqr > rangeSqr || distanceSqr >= nearestDistanceSqr) continue;

            nearest = bag;
            nearestDistanceSqr = distanceSqr;
        }

        return nearest;
    }

    private Inventory GetLootInventory()
    {
        return _lootContainerInventory != null ? _lootContainerInventory.GetComponent<Inventory>() : null;
    }

    private void ShowUI(Inventory lootInventory)
    {
        if (lootInventory == null) return;
        if (_inventoryUIRect != null) _inventoryUIRect.localScale = Vector3.one;
        isUIActive = true;
        if (lootInventory.GetCurrentLootBag() != this) lootInventory.ShowLoot(this);
        if (_spriteRenderer != null) _spriteRenderer.color = Color.red;
    }

    private void ClearLocalActiveState()
    {
        isUIActive = false;
        if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
    }

    private void Update()
    {
        if (_player == null || _lootContainerInventory == null) return;

        Inventory lootInventory = GetLootInventory();
        if (lootInventory == null) return;
        
        LootBag nearest = GetNearestInInteractionRange(_player.transform.position);
        bool ownsUI = nearest == this;

        if (ownsUI)
        {
            ShowUI(lootInventory);
        }
        else
        {
            ClearLocalActiveState();
            if (lootInventory.GetCurrentLootBag() == this && nearest == null && _inventoryUIRect != null)
            {
                _inventoryUIRect.localScale = Vector3.zero;
            }
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

        foreach (int itemId in rolledIds)
        {
            Item item = Database.Singleton.GetItem(itemId);

            if (item == null)
            {
                Debug.LogError($"LootBag could not find item with ID {itemId}.");
                continue;
            }
            
            _seededItems.Add(item);
        }
    }
    
    private void RefreshLootUI()
    {
        Inventory inventory = GetLootInventory();
        if (inventory == null) return;
        if (inventory.GetCurrentLootBag() != this) return;
        inventory.ShowLoot(this);
    }

    public void AddItem(Item item, bool refreshUI = false)
    {
        if (item == null) return;
        _seededItems.Add(item);
        if (refreshUI) RefreshLootUI();
    }

    public List<Item> GetLootItems() => _seededItems;

    public void TryDestroyIfEmpty()
    {
        if (_seededItems.Count > 0 || !DestroyIfNoItems) return;

        Inventory lootInventory = GetLootInventory();
        bool wasCurrentBag = lootInventory != null && lootInventory.GetCurrentLootBag() == this;

        ClearLocalActiveState();

        var nearestInteractionRange = GetNearestInInteractionRange(
            _player != null ? _player.transform.position : transform.position, 
            this
        );

        if (wasCurrentBag && _player != null)
        {
            LootBag nextBag = GetNearestInInteractionRange(_player.transform.position, this);
            if (nextBag != null)
            {
                nextBag.ShowUI(lootInventory);
            }
            else if (_inventoryUIRect != null)
            {
                _inventoryUIRect.localScale = Vector3.zero;
            }
        }
        else if (_inventoryUIRect != null && nearestInteractionRange == null)
        {
            _inventoryUIRect.localScale = Vector3.zero;
        }

        Destroy(gameObject);
    }

    public void RemoveItem(Item item)
    {
        if (item != null) _seededItems.Remove(item);
    }
}
