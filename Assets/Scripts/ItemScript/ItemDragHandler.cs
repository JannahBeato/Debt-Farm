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
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
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

        if (dropSlot == null || originalSlot == null)
        {
            ReturnToOriginalSlot(originalSlot);
            return;
        }

        if (dropSlot == originalSlot)
        {
            ReturnToOriginalSlot(originalSlot);
            return;
        }

        Item draggedItem = GetComponent<Item>();
        Item dropItem = dropSlot.currentItem != null ? dropSlot.currentItem.GetComponent<Item>() : null;

        if (draggedItem != null && dropItem != null && draggedItem.ID == dropItem.ID)
        {
            dropItem.AddToStack(Mathf.Max(1, draggedItem.quantity));

            originalSlot.currentItem = null;
            Destroy(gameObject);

            TrySelectHotbarSlot(dropSlot);
            return;
        }

        if (dropSlot.currentItem != null)
        {
            dropSlot.currentItem.transform.SetParent(originalSlot.transform);
            dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            originalSlot.currentItem = dropSlot.currentItem;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        transform.SetParent(dropSlot.transform);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        dropSlot.currentItem = gameObject;

        TrySelectHotbarSlot(dropSlot);
    }

    private void ReturnToOriginalSlot(Slot originalSlot)
    {
        transform.SetParent(originalParent);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        TrySelectHotbarSlot(originalSlot);
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

        if (slot.transform.parent != hotbar.hotbarPanel.transform)
            return;

        hotbar.SelectSlotByIndex(slot.transform.GetSiblingIndex());
    }
}
