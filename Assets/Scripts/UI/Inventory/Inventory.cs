using HeroesOfCrimson.Utils;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace UI.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Singleton;
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] public Transform draggablesTransform;
        [SerializeField] private InventoryItem itemPrefab;

        private Sprite[] _armorAndWeaponSprites;
        private Sprite[] _consumablesSprites;
        
        [SerializeField] public Sprite slotImage;
        
        [SerializeField] public Sprite hotbarWeaponImage;
        [SerializeField] public Sprite hotbarAbilityImage;
        [SerializeField] public Sprite hotbarArmorImage;
        [SerializeField] public Sprite hotbarAccessoryImage;

        private void Awake()
        {
            _armorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
            _consumablesSprites = Resources.LoadAll<Sprite>("Sprites/Items/consumables");
            Singleton = this;
        }

        private void Start()
        {
            if (name == "Hotbar")
            {
                var weaponItem = ScriptableObject.CreateInstance<Item>();
                var armorItem = ScriptableObject.CreateInstance<Item>();
                var ringItem = ScriptableObject.CreateInstance<Item>();

                weaponItem.sprite = _armorAndWeaponSprites[117];
                weaponItem.tag = Constants.SlotTag.Weapon;
                weaponItem.id = Utils.RandInt(1, 9999);
                
                armorItem.sprite = _armorAndWeaponSprites[27];
                armorItem.tag = Constants.SlotTag.Armor;
                armorItem.id = Utils.RandInt(1, 9999);
                
                ringItem.sprite = _armorAndWeaponSprites[127];
                ringItem.tag = Constants.SlotTag.Accessory;
                ringItem.id = Utils.RandInt(1, 9999);

                SpawnItem(weaponItem, 0);
                SpawnItem(armorItem, 2);
                SpawnItem(ringItem, 3);
            }
            else
            {
                var item1 = ScriptableObject.CreateInstance<Item>();
                var item2 = ScriptableObject.CreateInstance<Item>();
                var item3 = ScriptableObject.CreateInstance<Item>();

                item1.sprite = _consumablesSprites[0];
                item1.id = Utils.RandInt(1, 9999);
                
                item2.sprite = _consumablesSprites[1];
                item2.id = Utils.RandInt(1, 9999);
                
                item3.sprite = _consumablesSprites[2];
                item3.id = Utils.RandInt(1, 9999);

                SpawnItem(item1);
                SpawnItem(item2);
                SpawnItem(item3);
            }
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
                if (slot.CurrentInventoryItem != null) continue;
                var newItem = Instantiate(itemPrefab, slot.transform);
                newItem.Initialize(item, slot);
                break;
            }
            
        }
    }
}
