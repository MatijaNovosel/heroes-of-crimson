using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace UI.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] public Transform draggablesTransform;
        [SerializeField] private InventoryItem itemPrefab;
        
        private LootBag _currentLootBag;
        public bool IsLootInventory => _currentLootBag != null;
        public List<int> ItemIds => inventorySlots
            .Where(x => x.CurrentInventoryItem != null && x.CurrentInventoryItem.ItemInSlot != null)
            .Select(x => x.CurrentInventoryItem.ItemInSlot.id)
            .ToList();

        private void _setLootSource(LootBag bag)
        {
            _currentLootBag = bag;
        }

        public LootBag GetCurrentLootBag() => _currentLootBag;

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
            for (int i = 0; i < items.Count && i < inventorySlots.Length; i++) SpawnItem(items[i], i);
        }
        
        private void _clearInventory()
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.CurrentInventoryItem)
                {
                    Destroy(slot.CurrentInventoryItem.gameObject);
                    slot.CurrentInventoryItem = null;
                }
            }
        }

        public InventorySlot GetHotbarSlot(int idx)
        {
            return name == "Hotbar" ? inventorySlots[idx] : null;
        }

        public void SpawnItem(Item item = null, int? index = null)
        {
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
    }
}
