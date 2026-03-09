using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour, IInteractable
{
    public string shopID = "shop_merchant_01";
    public string shopKeeperName = "Merchant";

    public List<ShopStockItem> defaultShopStock = new List<ShopStockItem>();
    private List<ShopStockItem> currentShopStock = new List<ShopStockItem>();

    private bool isInitialized = false;

    [System.Serializable]
    public class ShopStockItem
    {
        public int itemID;
        public int quantity;
    }

    private void Start() 
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized) return;

        currentShopStock = new List<ShopStockItem>();
        foreach (var item in defaultShopStock)
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemID = item.itemID,
                quantity = item.quantity
            });
        }

        isInitialized = true;
    }

    public bool CanInteract() => true;

    public void Interact()
    {
        if (ShopController.Instance == null) return;

        if (ShopController.Instance.shopPanel.activeSelf)
            ShopController.Instance.CloseShop();
        else
            ShopController.Instance.OpenShop(this);
    }

    public List<ShopStockItem> GetCurrentStock() => currentShopStock;

    public void SetStock(List<ShopStockItem> stock) => currentShopStock = stock;

    public void AddToStock(int itemID, int quantity)
    {
        var existingItem = currentShopStock.Find(s => s.itemID == itemID);
        if (existingItem != null) existingItem.quantity += quantity;
        else currentShopStock.Add(new ShopStockItem { itemID = itemID, quantity = quantity });
    }

    public bool RemoveFromStock(int itemID, int quantity)
    {
        var existingItem = currentShopStock.Find(s => s.itemID == itemID);
        if (existingItem != null && existingItem.quantity >= quantity)
        {
            existingItem.quantity -= quantity;
            if (existingItem.quantity <= 0) currentShopStock.Remove(existingItem); // optional cleanup
            return true;
        }
        return false;
    }

    public ShopSaveData GetSaveData()
    {
        InitializeShop();

        ShopSaveData data = new ShopSaveData
        {
            shopID = shopID,
            stock = new List<ShopStockSaveData>()
        };

        foreach (var item in currentShopStock)
        {
            data.stock.Add(new ShopStockSaveData
            {
                itemID = item.itemID,
                quantity = item.quantity
            });
        }

        return data;
    }

    public void LoadFromSaveData(ShopSaveData data)
    {
        InitializeShop();

        if (data == null || data.stock == null) return;

        currentShopStock = new List<ShopStockItem>();

        foreach (var item in data.stock)
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemID = item.itemID,
                quantity = item.quantity
            });
        }

        isInitialized = true;
    }
}