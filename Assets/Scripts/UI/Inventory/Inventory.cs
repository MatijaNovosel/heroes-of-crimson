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
        
        [SerializeField] public Sprite slotImage;
        
        [SerializeField] public Sprite hotbarWeaponImage;
        [SerializeField] public Sprite hotbarAbilityImage;
        [SerializeField] public Sprite hotbarArmorImage;
        [SerializeField] public Sprite hotbarAccessoryImage;
        
        [SerializeField] public AudioClip errorSound;
        [SerializeField] public AudioClip moveSound;

        private void Start()
        {
            if (name == "Hotbar")
            {
                //
            }
            else
            {
                SpawnItem(Database.Singleton.GetItem(1));
                SpawnItem(Database.Singleton.GetItem(2));
                SpawnItem(Database.Singleton.GetItem(3));
                SpawnItem(Database.Singleton.GetItem(4));
                SpawnItem(Database.Singleton.GetItem(5));
                SpawnItem(Database.Singleton.GetItem(6));
                SpawnItem(Database.Singleton.GetItem(7));
            }
        }

        public void HandleInvalidAction()
        {
            AudioManager.Singleton.PlaySound(errorSound);
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
