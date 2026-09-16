using HeroesOfCrimson.Utils;
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
        [SerializeField] private float nearbyLootBagDistance = 1.5f;

        private Image _itemIcon;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        private Inventory _dragSourceInventory;
        private bool _dropHandled;

        public Item ItemInSlot { get; private set; }
        public InventorySlot ActiveSlot { get; set; }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemIcon = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _itemIcon.raycastTarget = true;
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
            if (ItemInSlot == null) return;

            bool shiftHeld =
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift);

            // Shift + click = quick equip
            if (shiftHeld)
            {
                Inventory inventory = ActiveSlot.GetComponentInParent<Inventory>();
                inventory?.TryQuickEquip(this);
                return;
            }

            // Double click = use consumable
            if (eventData.clickCount == 2)
            {
                ActiveSlot.TryUseConsumable();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dropHandled = false;
            _dragSourceInventory = ActiveSlot.GetComponentInParent<Inventory>();

            if (_dragSourceInventory == null) return;

            transform.SetParent(_dragSourceInventory.draggablesTransform);
            _canvasGroup.blocksRaycasts = false;
            _itemIcon.raycastTarget = false;

            Player.Singleton.HoldingItem = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = Input.mousePosition;

            if (_dragSourceInventory != null && transform.parent != _dragSourceInventory.draggablesTransform)
                transform.SetParent(_dragSourceInventory.draggablesTransform);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _itemIcon.raycastTarget = true;
            Player.Singleton.HoldingItem = false;

            if (!_dropHandled && TryDropIntoWorld()) return;

            transform.SetParent(ActiveSlot.transform, false);
            _rectTransform.anchoredPosition = Vector2.zero;
        }

        public void MarkDropHandled()
        {
            _dropHandled = true;
        }

        private bool TryDropIntoWorld()
        {
            if (_dragSourceInventory == null || _dragSourceInventory.IsLootInventory) return false;
            if (ItemInSlot == null || ActiveSlot == null) return false;

            Player player = Player.Singleton;
            if (!player) return false;

            Vector3 dropPosition = player.transform.position;

            LootBag lootBag = LootBag.GetNearest(dropPosition, nearbyLootBagDistance);
            bool createdNewBag = false;

            if (!lootBag)
            {
                GameObject lootBagPrefab = Resources.Load<GameObject>("Prefabs/LootBag");

                if (!lootBagPrefab)
                {
                    Debug.LogError("Could not find Resources/Prefabs/LootBag.");
                    return false;
                }

                GameObject bagObject = Instantiate(lootBagPrefab, dropPosition, Quaternion.identity);
                lootBag = bagObject.GetComponent<LootBag>();

                if (!lootBag)
                {
                    Debug.LogError("LootBag prefab does not contain a LootBag component.");
                    Destroy(bagObject);
                    return false;
                }

                lootBag.initialItemIds = System.Array.Empty<int>();
                createdNewBag = true;
            }

            lootBag.AddItem(ItemInSlot, true);

            ActiveSlot.SetItem(null);

            TooltipManager.Singleton.Hide();
            Destroy(gameObject);

            if (createdNewBag) AudioManager.Singleton.PlaySoundCached(Constants.Sounds.LootDrop);

            return true;
        }
    }
}