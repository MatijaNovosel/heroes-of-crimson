using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public InventoryItem CurrentInventoryItem { get; set; }
        public Constants.SlotTag Tag;

        public void OnDrop(PointerEventData eventData)
        {
            var droppedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
            if (!droppedItem) return;

            if (Tag != Constants.SlotTag.None && droppedItem.ItemInSlot.tag != Tag)
            {
                return;
            }

            var fromSlot = droppedItem.ActiveSlot;
            var toSlot = this;

            if (!toSlot.CurrentInventoryItem)
            {
                MoveItem(droppedItem, toSlot);
                return;
            }

            if (toSlot.CurrentInventoryItem.ItemInSlot.id == droppedItem.ItemInSlot.id) return;
        
            var otherItem = toSlot.CurrentInventoryItem;
            MoveItem(otherItem, fromSlot);
            MoveItem(droppedItem, toSlot);
        }

        private static void MoveItem(InventoryItem item, InventorySlot targetSlot)
        {
            if (item.ActiveSlot)
            {
                item.ActiveSlot.CurrentInventoryItem = null;
            }

            targetSlot.CurrentInventoryItem = item;
            item.ActiveSlot = targetSlot;

            item.transform.SetParent(targetSlot.transform, false);
            ((RectTransform)item.transform).anchoredPosition = Vector2.zero;
        }
    }
}
