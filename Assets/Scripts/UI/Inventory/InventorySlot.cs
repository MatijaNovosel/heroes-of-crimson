using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public InventoryItem CurrentItem { get; set; }
    public Constants.SlotTag Tag;

    public void OnDrop(PointerEventData eventData)
    {
        var droppedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
        if (droppedItem == null) return;

        if (Tag != Constants.SlotTag.None && droppedItem.CurrentItem.tag != Tag)
        {
            return;
        }

        var fromSlot = droppedItem.ActiveSlot;
        var toSlot = this;

        if (toSlot.CurrentItem == null)
        {
            MoveItem(droppedItem, toSlot);
            return;
        }

        if (toSlot.CurrentItem == null || toSlot.CurrentItem == droppedItem) return;
        
        var otherItem = toSlot.CurrentItem;
        MoveItem(otherItem, fromSlot);
        MoveItem(droppedItem, toSlot);
    }

    private void MoveItem(InventoryItem item, InventorySlot targetSlot)
    {
        if (item.ActiveSlot != null)
        {
            item.ActiveSlot.CurrentItem = null;
        }

        targetSlot.CurrentItem = item;
        item.ActiveSlot = targetSlot;

        item.transform.SetParent(targetSlot.transform, false);
        ((RectTransform)item.transform).anchoredPosition = Vector2.zero;
    }
}
