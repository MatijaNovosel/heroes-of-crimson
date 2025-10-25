using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _itemIcon;
    private CanvasGroup CanvasGroup { get; set; }
    public Item CurrentItem { get; set; }
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
        ActiveSlot.CurrentItem = this;
        CurrentItem = item;
        _itemIcon.sprite = item.sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(Inventory.Singleton.draggablesTransform);
        CanvasGroup.blocksRaycasts = false;
        _itemIcon.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = Input.mousePosition;
        if (transform.parent != Inventory.Singleton.draggablesTransform)
            transform.SetParent(Inventory.Singleton.draggablesTransform);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasGroup.blocksRaycasts = true;
        _itemIcon.raycastTarget = true;

        if (transform.parent == Inventory.Singleton.draggablesTransform)
        {
            transform.SetParent(ActiveSlot.transform);
            ((RectTransform)transform).anchoredPosition = Vector2.zero;
        }
    }
}