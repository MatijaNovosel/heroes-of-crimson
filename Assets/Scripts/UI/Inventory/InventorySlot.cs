using System;
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
    public Constants.SlotTag Tag;

    private Image _image;
    private Inventory _owner;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.raycastTarget = true;
        _owner = GetComponentInParent<Inventory>();
    }

    private void SetImage(Sprite sprite) => _image.sprite = sprite;

    public void ChangeImage(bool shouldRevertToDefault = false)
    {
        if (shouldRevertToDefault)
        {
            SetImage(_owner.slotImage);
            return;
        }
        
        switch (Tag)
        {
            case Constants.SlotTag.Weapon:
            {
                SetImage(_owner.hotbarWeaponImage);
                break;
            }
            case Constants.SlotTag.None:
            {
                SetImage(_owner.slotImage);
                break;
            }
            case Constants.SlotTag.Ability:
            {
                SetImage(_owner.hotbarAbilityImage);
                break;
            }
            case Constants.SlotTag.Armor:
            {
                SetImage(_owner.hotbarArmorImage);
                break;
            }
            case Constants.SlotTag.Accessory:
            {
                SetImage(_owner.hotbarAccessoryImage);
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
            AudioManager.Singleton.PlaySound(_owner.errorSound);
            return;
        }

        var fromSlot = droppedItem.ActiveSlot;
        var toSlot = this;

        if (toSlot.Tag == Constants.SlotTag.None && fromSlot.IsHotbar) fromSlot.ChangeImage();
        if (toSlot.IsHotbar && fromSlot.Tag == Constants.SlotTag.None) toSlot.ChangeImage(true);

        AudioManager.Singleton.PlaySound(_owner.moveSound);

        if (!toSlot.CurrentInventoryItem)
        {
            MoveItem(droppedItem, toSlot);
            return;
        }

        var otherItem = toSlot.CurrentInventoryItem;
        if (otherItem.ItemInSlot.id == droppedItem.ItemInSlot.id)
        {
            AudioManager.Singleton.PlaySound(_owner.errorSound);
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
