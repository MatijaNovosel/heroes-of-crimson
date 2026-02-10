using System.Collections.Generic;
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
                    (int)Constants.Character.Mage => 2000,
                    (int)Constants.Character.Knight => 2005,
                    (int)Constants.Character.Ranger => 2003,
                    _ => 2000
                };

                SpawnItem(Database.Singleton.GetItem(itemId), (int)Constants.InventorySlotEnum.Weapon);
                inventorySlots[(int)Constants.InventorySlotEnum.Weapon].RefreshVisual();
            }
            else if (name == "Inventory")
            {
                SpawnItem(Database.Singleton.GetItem(7000));
                SpawnItem(Database.Singleton.GetItem(7001));
                SpawnItem(Database.Singleton.GetItem(7002));
                SpawnItem(Database.Singleton.GetItem(6000));
                SpawnItem(Database.Singleton.GetItem(2009));
            }
        }
        
        public void ShowLoot(LootBag bag)
        {
            ClearInventory();
            _setLootSource(bag);
            var items = bag.GetLootItems();
            for (int i = 0; i < items.Count && i < inventorySlots.Length; i++) SpawnItem(items[i], i);
        }
        
        public void ClearInventory()
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
