using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public GameObject currentItem;
    public bool isShopSlot;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isShopSlot)
            return;

        HotbarController hotbar = GetComponentInParent<HotbarController>();
        if (hotbar == null || hotbar.hotbarPanel == null)
            return;

        // Only select if this slot is actually inside the hotbar panel
        if (transform.parent != hotbar.hotbarPanel.transform)
            return;

        hotbar.SelectSlotByIndex(transform.GetSiblingIndex());
    }
}