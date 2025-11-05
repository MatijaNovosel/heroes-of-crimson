using System;
using GameManagement;
using HeroesOfCrimson.Utils;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IDropHandler
{
    public InventoryItem CurrentInventoryItem { get; set; }
    public bool IsHotbar;
    public Constants.SlotTag Tag;

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
            SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Empty]);
            return;
        }
        
        switch (Tag)
        {
            case Constants.SlotTag.Weapon:
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Weapon]);
                break;
            }
            case Constants.SlotTag.None:
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Empty]);
                break;
            }
            case Constants.SlotTag.Ability:
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Ability]);
                break;
            }
            case Constants.SlotTag.Armor:
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Armor]);
                break;
            }
            case Constants.SlotTag.Accessory:
            {
                SetImage(ResourceCacher.Singleton.InventorySprites[Constants.InventorySprites.Accessory]);
                break;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
        if (!droppedItem) return;

        // enforce tags
        if (Tag != Constants.SlotTag.None && droppedItem.ItemInSlot.tag != Tag)
        {
            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
            return;
        }

        var fromSlot = droppedItem.ActiveSlot;
        var toSlot = this;

        if (toSlot.Tag == Constants.SlotTag.None && fromSlot.IsHotbar) fromSlot.ChangeImage();
        if (toSlot.IsHotbar && fromSlot.Tag == Constants.SlotTag.None) toSlot.ChangeImage(true);

        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.InventoryMove);

        if (!toSlot.CurrentInventoryItem)
        {
            MoveItem(droppedItem, toSlot);
            return;
        }

        var otherItem = toSlot.CurrentInventoryItem;
        if (otherItem.ItemInSlot.id == droppedItem.ItemInSlot.id)
        {
            AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
            return;
        }

        MoveItem(otherItem, fromSlot);
        MoveItem(droppedItem, toSlot);
    }

    private static void MoveItem(InventoryItem item, InventorySlot targetSlot)
    {
        if (item.ActiveSlot) item.ActiveSlot.CurrentInventoryItem = null;

        targetSlot.CurrentInventoryItem = item;
        item.ActiveSlot = targetSlot;

        item.transform.SetParent(targetSlot.transform, false);
        ((RectTransform)item.transform).anchoredPosition = Vector2.zero;
    }
}

}
