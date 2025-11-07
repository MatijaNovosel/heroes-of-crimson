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

        private void Start()
        {
            if (name == "Hotbar")
            {
                //
            }
            else if (name == "Inventory")
            {
                SpawnItem(Database.Singleton.GetItem(2000));
                SpawnItem(Database.Singleton.GetItem(5000));
                SpawnItem(Database.Singleton.GetItem(5001));
                SpawnItem(Database.Singleton.GetItem(5002));
                SpawnItem(Database.Singleton.GetItem(5003));
            }
        }

        public InventorySlot GetHotbarSlot(int idx)
        {
            return name == "Hotbar" ? inventorySlots[idx] : null;
        }

        private void SpawnItem(Item item = null, int? index = null)
        {
            if (index != null)
            {
                var slot = inventorySlots[(int)index];
                if (slot.CurrentInventoryItem) return;
                slot.ChangeImage(true);
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
