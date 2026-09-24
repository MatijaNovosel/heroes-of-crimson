using UI;
using UI.Inventory;
using UnityEngine;

public class ConsumableQuickSlots : MonoBehaviour
{
    public static ConsumableQuickSlots Singleton { get; private set; }

    [SerializeField] private InventorySlot leftSlot;
    [SerializeField] private InventorySlot rightSlot;
    [SerializeField] private KeyCode leftKey = KeyCode.Q;
    [SerializeField] private KeyCode rightKey = KeyCode.E;

    public Inventory SlotInventory { get; private set; }

    private void Awake()
    {
        Singleton = this;
        SlotInventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        if (!SlotInventory.draggablesTransform && Inventory.PlayerInventory)
        {
            SlotInventory.draggablesTransform = Inventory.PlayerInventory.draggablesTransform;
        }

        if (leftSlot) leftSlot.RefreshVisual();
        if (rightSlot) rightSlot.RefreshVisual();
    }

    private void Update()
    {
        if (IsBlocked()) return;
        if (Input.GetKeyDown(leftKey)) Use(leftSlot);
        if (Input.GetKeyDown(rightKey)) Use(rightSlot);
    }

    private static void Use(InventorySlot slot)
    {
        if (slot) slot.TryUseConsumable();
    }

    private static bool IsBlocked()
    {
        var player = Player.Singleton;
        if (!player || player.HoldingItem) return true;

        return (ConsoleMenu.Singleton && ConsoleMenu.Singleton.ConsoleMenuOpen)
               || (PauseMenu.Singleton && PauseMenu.Singleton.PauseMenuOpen)
               || (TalentTree.Singleton && TalentTree.Singleton.TalentTreeOpen)
               || (DialogMenu.Singleton && DialogMenu.Singleton.DialogMenuOpen);
    }
}
