using Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _itemIcon;
        private CanvasGroup CanvasGroup { get; set; }
        public Item ItemInSlot { get; set; }
        public InventorySlot ActiveSlot { get; set; }
        private RectTransform _rectTransform;

        private void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            _itemIcon = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(Item item, InventorySlot parent)
        {
            ActiveSlot = parent;
            ActiveSlot.CurrentInventoryItem = this;
            ItemInSlot = item;
            _itemIcon.sprite = item.sprite;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ItemInSlot) return;
            
            TooltipManager.Singleton.SetInfo(
                ItemInSlot.name,
                ItemInSlot.description,
                ItemInSlot.tag,
                ItemInSlot.rarity,
                ItemInSlot.minDamage,
                ItemInSlot.maxDamage,
                ItemInSlot.stats
            );
            
            TooltipManager.Singleton.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Singleton.Hide();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetParent(Inventory.Singleton.draggablesTransform);
            CanvasGroup.blocksRaycasts = false;
            _itemIcon.raycastTarget = false;
            Inventory.Singleton.HoldingItem = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = Input.mousePosition;
            if (transform.parent != Inventory.Singleton.draggablesTransform)
            {
                transform.SetParent(Inventory.Singleton.draggablesTransform);
            }
        }
    
        public void OnEndDrag(PointerEventData eventData)
        {
            CanvasGroup.blocksRaycasts = true;
            _itemIcon.raycastTarget = true;
            Inventory.Singleton.HoldingItem = false;
            
            if (transform.parent != Inventory.Singleton.draggablesTransform) return;
            transform.SetParent(ActiveSlot.transform);
            ((RectTransform)transform).anchoredPosition = Vector2.zero;
        }
    }
}