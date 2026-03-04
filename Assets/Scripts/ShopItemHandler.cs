using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isShopItem;

    // The REAL inventory slot this UI entry represents (only set for player-in-shop UI)
    public Slot originalInventorySlot;

    public void Initialize(bool shopItem, Slot originalSlot = null)
    {
        isShopItem = shopItem;
        originalInventorySlot = originalSlot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        if (isShopItem) BuyItem();
        else SellItem();
    }

    private void BuyItem()
    {
        var ctx = GetComponentInParent<ShopUISlotContext>();
        if (ctx == null) return;

        int itemID = ctx.itemID;
        int price = ctx.price;

        if (CurrencyController.Instance == null || ShopController.Instance == null) return;

        if (CurrencyController.Instance.GetGold() < price)
        {
            Debug.Log("Not enough gold to buy this item!");
            return;
        }

        var dict = FindObjectOfType<ItemDictionary>();
        if (dict == null)
        {
            Debug.LogError("ShopItemHandler: No ItemDictionary found.");
            return;
        }

        GameObject itemPrefab = dict.GetItemPrefab(itemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"ShopItemHandler: No item prefab for itemID {itemID}");
            return;
        }

        if (InventoryController.Instance == null)
        {
            Debug.LogError("ShopItemHandler: InventoryController.Instance missing.");
            return;
        }

        // Add to inventory (supports stacking)
        if (InventoryController.Instance.AddItem(itemPrefab))
        {
            CurrencyController.Instance.SpendGold(price);

            // Remove 1 from shop and refresh UI
            ShopController.Instance.RemoveItemFromShop(itemID, 1);
            ShopController.Instance.RefreshPlayerInventoryDisplay();
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }

    private void SellItem()
    {
        var ctx = GetComponentInParent<ShopUISlotContext>();
        if (ctx == null) return;

        if (CurrencyController.Instance == null || ShopController.Instance == null || InventoryController.Instance == null)
            return;

        if (originalInventorySlot == null)
        {
            Debug.LogWarning("ShopItemHandler: originalInventorySlot is null (player item sell UI was not linked).");
            return;
        }

        Item invItem = originalInventorySlot.currentItem != null
            ? originalInventorySlot.currentItem.GetComponent<Item>()
            : null;

        if (invItem == null) return;

        int sellPrice = ctx.price;
        int itemID = invItem.ID;

        // Remove 1 from player's inventory slot
        if (invItem.quantity > 1)
        {
            invItem.RemoveFromStack(1);
        }
        else
        {
            Destroy(originalInventorySlot.currentItem);
            originalInventorySlot.currentItem = null;
        }

        // Merge duplicates + refresh quantity texts
        InventoryController.Instance.RebuildItemCounts();

        // Pay player + add item to shop stock
        CurrencyController.Instance.AddGold(sellPrice);
        ShopController.Instance.AddItemToShop(itemID, 1);

        // Refresh shop UI panels
        ShopController.Instance.RefreshPlayerInventoryDisplay();
        ShopController.Instance.RefreshShopDisplay();
    }
}