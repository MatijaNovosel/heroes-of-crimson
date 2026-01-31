using GameManagement;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
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
        
        public void OnPointerClick(PointerEventData data)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            var player = playerObj.GetComponent<Player>();
            
            if (data.clickCount == 2)
            {
                if (CurrentInventoryItem != null && CurrentInventoryItem.ItemInSlot != null)
                {
                    var item = CurrentInventoryItem.ItemInSlot;

                    if (item.tag == Constants.ItemTag.Consumable)
                    {
                        switch (item.id)
                        {
                            case (int)Constants.ConsumableItem.HpPot:
                            {
                                player.RestoreHp(50);
                                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.UsePotion);
                                break;
                            }
                            case (int)Constants.ConsumableItem.ManaPot:
                            {
                                player.RestoreMp(30);
                                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.UsePotion);
                                break;
                            }
                        }
                        
                        var itemUI = CurrentInventoryItem;
                        CurrentInventoryItem = null;
                        Destroy(itemUI.gameObject);
                        RefreshVisual();
                        TooltipManager.Singleton.Hide();
                    }
                }
            }
        }
        
        public void RefreshVisual()
        {
            if (CurrentInventoryItem != null)
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Empty]);
                return;
            }

            switch (Tag)
            {
                case Constants.ItemTag.Weapon:
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Weapon]);
                    break;
                case Constants.ItemTag.Ability:
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Ability]);
                    break;
                case Constants.ItemTag.Armor:
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Armor]);
                    break;
                case Constants.ItemTag.Accessory:
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Accessory]);
                    break;
                case Constants.ItemTag.None:
                default:
                    SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySlotEnum.Empty]);
                    break;
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

            var fromInventory = fromSlot.GetComponentInParent<Inventory>();
            var toInventory = GetComponentInParent<Inventory>();

            bool fromLoot = fromInventory != null && fromInventory.IsLootInventory;
            bool toLoot = toInventory != null && toInventory.IsLootInventory;

            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.InventoryMove);

            // MOVE (empty target)
            if (toSlot.CurrentInventoryItem is null)
            {
                if (fromLoot && !toLoot) fromInventory.GetCurrentLootBag()?.RemoveItem(droppedItem.ItemInSlot);
                if (!fromLoot && toLoot) toInventory.GetCurrentLootBag()?.AddItem(droppedItem.ItemInSlot);
                MoveItem(droppedItem, toSlot);
                return;
            }

            // SWAP
            var targetItem = toSlot.CurrentInventoryItem;

            if (fromLoot && !toLoot)
            {
                var bag = fromInventory.GetCurrentLootBag();
                bag?.RemoveItem(droppedItem.ItemInSlot);
                bag?.AddItem(targetItem.ItemInSlot);
            }
            else if (!fromLoot && toLoot)
            {
                var bag = toInventory.GetCurrentLootBag();
                bag?.RemoveItem(targetItem.ItemInSlot);
                bag?.AddItem(droppedItem.ItemInSlot);
            }

            SwapItems(fromSlot, toSlot);
        }
        
        private static void MoveItem(InventoryItem item, InventorySlot targetSlot)
        {
            var previousSlot = item.ActiveSlot;

            if (previousSlot)
            {
                previousSlot.CurrentInventoryItem = null;
                previousSlot.RefreshVisual();
            }

            item.ActiveSlot = targetSlot;
            targetSlot.CurrentInventoryItem = item;

            item.transform.SetParent(targetSlot.transform, false);
            ((RectTransform)item.transform).anchoredPosition = Vector2.zero;

            targetSlot.RefreshVisual();
        }

        
        private static void SwapItems(InventorySlot slotA, InventorySlot slotB)
        {
            var itemA = slotA.CurrentInventoryItem;
            var itemB = slotB.CurrentInventoryItem;

            slotA.CurrentInventoryItem = itemB;
            slotB.CurrentInventoryItem = itemA;

            if (itemB != null)
            {
                itemB.ActiveSlot = slotA;
                itemB.transform.SetParent(slotA.transform, false);
                ((RectTransform)itemB.transform).anchoredPosition = Vector2.zero;
            }

            if (itemA != null)
            {
                itemA.ActiveSlot = slotB;
                itemA.transform.SetParent(slotB.transform, false);
                ((RectTransform)itemA.transform).anchoredPosition = Vector2.zero;
            }

            slotA.RefreshVisual();
            slotB.RefreshVisual();
        }

    }
}
