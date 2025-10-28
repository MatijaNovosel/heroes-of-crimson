using System;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public InventoryItem CurrentInventoryItem { get; set; }
        public bool IsHotbar;
        private Image _image;
        
        public Constants.SlotTag Tag;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CurrentInventoryItem && CurrentInventoryItem.ItemInSlot)
            {
                TooltipManager.Singleton.SetInfo(
                    CurrentInventoryItem.ItemInSlot.name,
                    CurrentInventoryItem.ItemInSlot.description
                );
                TooltipManager.Singleton.Show();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Singleton.Hide();
        }

        public void ChangeImage(bool shouldRevertToDefault = false)
        {
            if (shouldRevertToDefault)
            {
                SetImage(Inventory.Singleton.slotImage);
                return;
            }
            
            switch (Tag)
            {
                case Constants.SlotTag.Weapon:
                    SetImage(Inventory.Singleton.hotbarWeaponImage);
                    break;
                case Constants.SlotTag.None:
                    SetImage(Inventory.Singleton.slotImage);
                    break;
                case Constants.SlotTag.Ability:
                    SetImage(Inventory.Singleton.hotbarAbilityImage);
                    break;
                case Constants.SlotTag.Armor:
                    SetImage(Inventory.Singleton.hotbarArmorImage);
                    break;
                case Constants.SlotTag.Accessory:
                    SetImage(Inventory.Singleton.hotbarAccessoryImage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var droppedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
            if (!droppedItem)
            {
                return;
            }

            // Do not mismatch tags - look @ Slot tag enum
            if (Tag != Constants.SlotTag.None && droppedItem.ItemInSlot.tag != Tag)
            {
                Inventory.Singleton.HandleInvalidAction();
                return;
            }
            
            var fromSlot = droppedItem.ActiveSlot;
            var toSlot = this;
            
            if (toSlot.Tag == Constants.SlotTag.None && fromSlot.IsHotbar)
            {
                fromSlot.ChangeImage();
            }
            
            if (toSlot.IsHotbar && fromSlot.Tag == Constants.SlotTag.None)
            {
                toSlot.ChangeImage(true);
            }
            
            AudioManager.Singleton.PlaySound(Inventory.Singleton.moveSound);

            if (!toSlot.CurrentInventoryItem)
            {
                MoveItem(droppedItem, toSlot);
                return;
            }
        
            var otherItem = toSlot.CurrentInventoryItem;

            if (toSlot.CurrentInventoryItem.ItemInSlot.id == droppedItem.ItemInSlot.id)
            {
                Inventory.Singleton.HandleInvalidAction();
                return;
            }
            
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
