using GameManagement;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public InventoryItem CurrentInventoryItem { get; set; }
        public bool IsHotbar;
        public Constants.ItemTag Tag;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.raycastTarget = true;
        }

        private void SetImage(Sprite sprite) => _image.sprite = sprite;

        public void ChangeImage(bool shouldRevertToDefault = false)
        {
            if (shouldRevertToDefault)
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Empty]);
                return;
            }
            
            switch (Tag)
            {
                case Constants.ItemTag.Weapon:
                {
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Weapon]);
                    break;
                }
                case Constants.ItemTag.None:
                {
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Empty]);
                    break;
                }
                case Constants.ItemTag.Ability:
                {
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Ability]);
                    break;
                }
                case Constants.ItemTag.Armor:
                {
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Armor]);
                    break;
                }
                case Constants.ItemTag.Accessory:
                {
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotSpritesEnum.Accessory]);
                    break;
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var droppedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
            if (droppedItem is null) return;

            if (Tag != Constants.ItemTag.None && droppedItem.ItemInSlot.tag != Tag)
            {
                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
                return;
            }

            var fromSlot = droppedItem.ActiveSlot;
            var toSlot = this;

            if (toSlot.Tag == Constants.ItemTag.None && fromSlot.IsHotbar) fromSlot.ChangeImage();
            if (toSlot.IsHotbar && fromSlot.Tag == Constants.ItemTag.None) toSlot.ChangeImage(true);

            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.InventoryMove);

            if (toSlot.CurrentInventoryItem is null)
            {
                MoveItem(droppedItem, toSlot);
                return;
            }

            SwapItems(fromSlot, toSlot);
        }

        private static void MoveItem(InventoryItem item, InventorySlot targetSlot)
        {
            if (item.ActiveSlot) item.ActiveSlot.CurrentInventoryItem = null;

            item.ActiveSlot = targetSlot;
            targetSlot.CurrentInventoryItem = item;

            item.transform.SetParent(targetSlot.transform, false);
            ((RectTransform)item.transform).anchoredPosition = Vector2.zero;
        }
        
        private static void SwapItems(InventorySlot slotA, InventorySlot slotB)
        {
            (slotA.CurrentInventoryItem, slotB.CurrentInventoryItem) = (slotB.CurrentInventoryItem, slotA.CurrentInventoryItem);

            if (slotA.CurrentInventoryItem != null)
            {
                slotA.CurrentInventoryItem.ActiveSlot = slotA;
                slotA.CurrentInventoryItem.transform.SetParent(slotA.transform, false);
                ((RectTransform)slotA.CurrentInventoryItem.transform).anchoredPosition = Vector2.zero;
            }

            if (slotB.CurrentInventoryItem == null) return;
            
            slotB.CurrentInventoryItem.ActiveSlot = slotB;
            slotB.CurrentInventoryItem.transform.SetParent(slotB.transform, false);
            ((RectTransform)slotB.CurrentInventoryItem.transform).anchoredPosition = Vector2.zero;
        }
    }
}
