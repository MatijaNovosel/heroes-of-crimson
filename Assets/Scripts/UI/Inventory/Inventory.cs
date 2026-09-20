using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

namespace UI.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] public Transform draggablesTransform;
        [SerializeField] private InventoryItem itemPrefab;

        private LootBag _currentLootBag;

        public static Inventory PlayerInventory { get; private set; }

        public bool IsLootInventory => _currentLootBag != null;
        public bool IsPlayerInventory => PlayerInventory == this;

        public List<int> ItemIds => inventorySlots
            .Where(x => x.CurrentInventoryItem != null && x.CurrentInventoryItem.ItemInSlot != null)
            .Select(x => x.CurrentInventoryItem.ItemInSlot.id)
            .ToList();

        private void _setLootSource(LootBag bag) => _currentLootBag = bag;

        public LootBag GetCurrentLootBag() => _currentLootBag;

        private void Awake()
        {
            var childSlots = GetComponentsInChildren<InventorySlot>(true);

            if (childSlots.Length > 0 && (inventorySlots == null || inventorySlots.Length != childSlots.Length))
            {
                inventorySlots = childSlots;
            }

            if (name == "Inventory") PlayerInventory = this;
        }

        private void Start()
        {
            if (name == "Hotbar")
            {
                int itemId = GameManager.Singleton.GetSelectedCharacter() switch
                {
                    (int)Constants.Character.Mage => (int)WeaponItemEnum.WhiteStaff,
                    (int)Constants.Character.Knight => (int)WeaponItemEnum.IronSword,
                    (int)Constants.Character.Ranger => (int)WeaponItemEnum.WoodenBow,
                    _ => (int)WeaponItemEnum.WhiteStaff
                };

                SpawnItem(Database.Singleton.GetItem(itemId), (int)Constants.InventorySlotEnum.Weapon);
                inventorySlots[(int)Constants.InventorySlotEnum.Weapon].RefreshVisual();
            }
            else if (name == "Inventory")
            {
                SpawnItem(Database.Singleton.GetItem((int)ConsumableItemEnum.HealthPotion));
                SpawnItem(Database.Singleton.GetItem((int)ConsumableItemEnum.ManaPotion));
                SpawnItem(Database.Singleton.GetItem((int)ConsumableItemEnum.PotionOfLife));
                SpawnItem(Database.Singleton.GetItem((int)WeaponItemEnum.Radiance));
            }
        }

        public void ShowLoot(LootBag bag)
        {
            _clearInventory();
            _setLootSource(bag);

            var items = bag.GetLootItems();

            for (int i = 0; i < items.Count && i < inventorySlots.Length; i++)
            {
                if (items[i] == null) continue;
                SpawnItem(items[i], i);
            }
        }

        private void _clearInventory()
        {
            foreach (var slot in inventorySlots)
            {
                if (!slot.CurrentInventoryItem) continue;
                Destroy(slot.CurrentInventoryItem.gameObject);
                slot.CurrentInventoryItem = null;
            }
        }

        public InventorySlot GetHotbarSlot(int idx) => name == "Hotbar" ? inventorySlots[idx] : null;

        public void SyncCurrentLootBagLayout()
        {
            if (_currentLootBag == null) return;
            var slotItems = new List<Item>(inventorySlots.Length);

            foreach (InventorySlot slot in inventorySlots)
            {
                slotItems.Add(slot.CurrentInventoryItem != null ? slot.CurrentInventoryItem.ItemInSlot : null);
            }

            _currentLootBag.SetSlotItems(slotItems);
        }

        public InventorySlot GetFirstAvailableSlot(Item item)
        {
            if (item == null) return null;

            foreach (var slot in inventorySlots)
            {
                if (slot.CurrentInventoryItem != null) continue;
                if (slot.Tag != Constants.ItemTag.None && slot.Tag != item.tag) continue;
                return slot;
            }

            return null;
        }

        public bool TryQuickTransferToPlayerInventory(InventoryItem item)
        {
            if (item == null || item.ItemInSlot == null || item.ActiveSlot == null) return false;
            if (PlayerInventory == null || PlayerInventory == this || name == "Hotbar") return false;

            InventorySlot targetSlot = PlayerInventory.GetFirstAvailableSlot(item.ItemInSlot);

            if (targetSlot == null)
            {
                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
                return false;
            }

            MoveItem(item, targetSlot);

            if (IsLootInventory) SyncCurrentLootBagLayout();

            GetCurrentLootBag()?.TryDestroyIfEmpty();

            TooltipManager.Singleton.Hide();
            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.InventoryMove);
            return true;
        }

        public InventorySlot GetEquipmentSlot(Constants.ItemTag tag)
        {
            if (name != "Hotbar") return null;

            InventorySlot firstMatchingSlot = null;

            foreach (var slot in inventorySlots)
            {
                if (slot.Tag != tag) continue;
                if (slot.CurrentInventoryItem == null) return slot;
                firstMatchingSlot ??= slot;
            }

            return firstMatchingSlot;
        }

        public bool TryQuickEquip(InventoryItem item)
        {
            if (item == null || item.ItemInSlot == null || item.ActiveSlot == null) return false;

            Constants.ItemTag tag = item.ItemInSlot.tag;

            if (tag != Constants.ItemTag.Weapon && tag != Constants.ItemTag.Armor && tag != Constants.ItemTag.Accessory)
            {
                return false;
            }

            Inventory hotbar = Player.Singleton.Hotbar;
            if (hotbar == null) return false;

            InventorySlot fromSlot = item.ActiveSlot;
            Inventory fromInventory = fromSlot.GetComponentInParent<Inventory>();

            if (fromInventory == hotbar) return false;

            InventorySlot targetSlot = hotbar.GetEquipmentSlot(tag);

            if (targetSlot == null)
            {
                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
                return false;
            }

            InventoryItem previouslyEquipped = targetSlot.CurrentInventoryItem;
            bool fromLoot = fromInventory != null && fromInventory.IsLootInventory;

            if (previouslyEquipped == null) MoveItem(item, targetSlot);
            else SwapItems(fromSlot, targetSlot);

            if (fromLoot) fromInventory.SyncCurrentLootBagLayout();
            fromInventory?.GetCurrentLootBag()?.TryDestroyIfEmpty();

            TooltipManager.Singleton.Hide();
            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.InventoryEquip);
            return true;
        }

        public void SpawnItem(Item item = null, int? index = null)
        {
            if (item == null) return;

            if (index != null)
            {
                var slot = inventorySlots[(int)index];
                if (slot.CurrentInventoryItem) return;
                var newItem = Instantiate(itemPrefab, slot.transform);
                newItem.Initialize(item, slot);
                return;
            }

            foreach (var slot in inventorySlots)
            {
                if (slot.CurrentInventoryItem) continue;
                var newItem = Instantiate(itemPrefab, slot.transform);
                newItem.Initialize(item, slot);
                break;
            }
        }

        private static void MoveItem(InventoryItem item, InventorySlot targetSlot)
        {
            InventorySlot previousSlot = item.ActiveSlot;
            previousSlot?.SetItem(null);
            item.ActiveSlot = targetSlot;
            targetSlot.SetItem(item);
            item.transform.SetParent(targetSlot.transform, false);
            ((RectTransform)item.transform).anchoredPosition = Vector2.zero;
        }

        private static void SwapItems(InventorySlot slotA, InventorySlot slotB)
        {
            InventoryItem itemA = slotA.CurrentInventoryItem;
            InventoryItem itemB = slotB.CurrentInventoryItem;

            slotA.SetItem(itemB);
            slotB.SetItem(itemA);

            if (itemA != null)
            {
                itemA.ActiveSlot = slotB;
                itemA.transform.SetParent(slotB.transform, false);
                ((RectTransform)itemA.transform).anchoredPosition = Vector2.zero;
            }

            if (itemB != null)
            {
                itemB.ActiveSlot = slotA;
                itemB.transform.SetParent(slotA.transform, false);
                ((RectTransform)itemB.transform).anchoredPosition = Vector2.zero;
            }
        }
    }
}
