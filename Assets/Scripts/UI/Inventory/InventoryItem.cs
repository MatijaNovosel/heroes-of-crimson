using Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventoryItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image _itemIcon;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        public Item ItemInSlot { get; private set; }
        public InventorySlot ActiveSlot { get; set; }

        private Inventory _owner;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemIcon = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _itemIcon.raycastTarget = true;
            _owner = GetComponentInParent<Inventory>();
        }

        public void Initialize(Item item, InventorySlot parent)
        {
            parent.CurrentInventoryItem = this;
            ActiveSlot = parent;
            ItemInSlot = item;
            _itemIcon.sprite = item.sprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ItemInSlot) return;

            TooltipManager.Singleton.SetInfo(ItemInSlot);
            TooltipManager.Singleton.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Singleton.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return;
            if (ItemInSlot == null) return;
            _owner.TryQuickEquip(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetParent(_owner.draggablesTransform);
            _canvasGroup.blocksRaycasts = false;
            _itemIcon.raycastTarget = false;
            Player.Singleton.HoldingItem = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = Input.mousePosition;

            if (transform.parent != _owner.draggablesTransform) transform.SetParent(_owner.draggablesTransform);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _itemIcon.raycastTarget = true;
            Player.Singleton.HoldingItem = false;

            transform.SetParent(ActiveSlot.transform, false);
            _rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}