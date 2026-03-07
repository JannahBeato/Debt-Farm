using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public GameObject currentItem;
    public bool isShopSlot;

    public bool IsEmpty => currentItem == null;

    public Item CurrentItemData => currentItem != null ? currentItem.GetComponent<Item>() : null;

    public void ClearSlot()
    {
        currentItem = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isShopSlot)
            return;

        HotbarController hotbar = GetComponentInParent<HotbarController>();
        if (hotbar == null || hotbar.hotbarPanel == null)
            return;

        if (transform.parent != hotbar.hotbarPanel.transform)
            return;

        hotbar.SelectSlotByIndex(transform.GetSiblingIndex());
    }
}
