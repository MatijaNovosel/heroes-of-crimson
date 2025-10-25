using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Singleton;
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] public Transform draggablesTransform;
    [SerializeField] private InventoryItem itemPrefab;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        var sprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
        var consumables = Resources.LoadAll<Sprite>("Sprites/Items/consumables");

        if (name == "Hotbar")
        {
            var weaponItem = ScriptableObject.CreateInstance<Item>();
            var armorItem = ScriptableObject.CreateInstance<Item>();
            var ringItem = ScriptableObject.CreateInstance<Item>();

            weaponItem.sprite = sprites[117];
            armorItem.sprite = sprites[27];
            ringItem.sprite = sprites[127];

            SpawnItem(weaponItem, 0);
            SpawnItem(armorItem, 2);
            SpawnItem(ringItem, 3);
        }
        else
        {
            var item1 = ScriptableObject.CreateInstance<Item>();
            var item2 = ScriptableObject.CreateInstance<Item>();
            var item3 = ScriptableObject.CreateInstance<Item>();

            item1.sprite = consumables[0];
            item2.sprite = consumables[1];
            item3.sprite = consumables[2];

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
            if (slot.CurrentItem) return;

            var newItem = Instantiate(itemPrefab, slot.transform);
            newItem.Initialize(item, slot);
            return;
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.CurrentItem != null) continue;
            var newItem = Instantiate(itemPrefab, slot.transform);
            newItem.Initialize(item, slot);
            break;
        }
    }
}
