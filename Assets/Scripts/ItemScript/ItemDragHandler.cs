using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Save original parent
        transform.SetParent(transform.root); // Move above other UI
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // Follow mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null)
        {
            GameObject dropObject = eventData.pointerEnter;
            if (dropObject != null)
                dropSlot = dropObject.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                // Swap items
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                if (originalSlot != null)
                    originalSlot.currentItem = null;
            }

            // Move dragged item into new slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;

            TrySelectHotbarSlot(dropSlot);
        }
        else
        {
            // Return to original slot
            transform.SetParent(originalParent);
            TrySelectHotbarSlot(originalSlot);
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Slot parentSlot = GetComponentInParent<Slot>();
        TrySelectHotbarSlot(parentSlot);
    }

    private void TrySelectHotbarSlot(Slot slot)
    {
        if (slot == null || slot.isShopSlot)
            return;

        HotbarController hotbar = slot.GetComponentInParent<HotbarController>();
        if (hotbar == null || hotbar.hotbarPanel == null)
            return;

        // Only select if this slot is directly under the hotbar panel
        if (slot.transform.parent != hotbar.hotbarPanel.transform)
            return;

        hotbar.SelectSlotByIndex(slot.transform.GetSiblingIndex());
    }
}