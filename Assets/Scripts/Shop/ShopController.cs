using System;
using System.Reflection;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid, playerInventoryGrid;
    public GameObject shopSlotPrefab;
    public TMP_Text playerMoneyText, shopTitleText;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        if (itemDictionary == null)
            Debug.LogError("ShopController: No ItemDictionary found in the scene! Add it to a GameObject.");

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (CurrencyController.Instance != null)
            CurrencyController.Instance.OnGoldChanged -= UpdateMoneyDisplay;
    }

    private void UpdateMoneyDisplay(int amount)
    {
        if (playerMoneyText != null)
            playerMoneyText.text = amount.ToString();
    }

    public bool IsShopOpen => shopPanel != null && shopPanel.activeSelf;

    public void OpenShop(ShopNPC shop)
    {
        if (shop == null) return;

        currentShop = shop;

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (shopTitleText != null)
            shopTitleText.text = $"{shop.shopKeeperName}'s Shop";

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();

        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        currentShop = null;

        PauseController.SetPause(false);
    }

    public void RefreshShopDisplay()
    {
        if (currentShop == null || shopInventoryGrid == null) return;

        ClearChildren(shopInventoryGrid);

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            if (stockItem == null || stockItem.quantity <= 0) continue;
            CreateShopSlot(shopInventoryGrid, stockItem.itemID, stockItem.quantity, isShop: true);
        }
    }

    public void RefreshPlayerInventoryDisplay()
    {
        if (playerInventoryGrid == null) return;

        // Prefer singleton, fallback just in case
        var inv = InventoryController.Instance != null
            ? InventoryController.Instance
            : FindObjectOfType<InventoryController>();

        if (inv == null || inv.inventoryPanel == null) return;

        ClearChildren(playerInventoryGrid);

        foreach (Transform slotTransform in inv.inventoryPanel.transform)
        {
            var inventorySlot = slotTransform.GetComponent<Slot>();
            if (inventorySlot == null || inventorySlot.currentItem == null) continue;

            var item = inventorySlot.currentItem.GetComponent<Item>();
            if (item == null) continue;

            int quantity = GetIntMember(item, "quantity", fallback: 1);
            CreateShopSlot(playerInventoryGrid, item.ID, quantity, isShop: false, originalSlot: inventorySlot);
        }
    }

    private void CreateShopSlot(Transform grid, int itemID, int quantity, bool isShop, Slot originalSlot = null)
    {
        if (grid == null || shopSlotPrefab == null || itemDictionary == null) return;

        GameObject slotObj = Instantiate(shopSlotPrefab, grid);

        
        var slotComponent = slotObj.GetComponent<Slot>();
        if (slotComponent == null) slotComponent = slotObj.AddComponent<Slot>();

        
        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"ShopController: No item prefab found for itemID {itemID}");
            Destroy(slotObj);
            return;
        }

        
        GameObject itemInstance = Instantiate(itemPrefab, slotObj.transform);
        slotComponent.currentItem = itemInstance;

        var rt = itemInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        itemInstance.transform.localPosition = Vector3.zero;
        itemInstance.transform.localScale = Vector3.one;

        
        var item = itemInstance.GetComponent<Item>();
        if (item != null)
        {
            SetIntMember(item, "quantity", quantity);
            InvokeIfExists(item, "UpdateQuantityDisplay");
        }

        
        int price = 0;
        if (item != null)
        {
            if (isShop)
                price = GetIntMember(item, "buyPrice", fallback: 0);
            else
                price = InvokeIntMethod(item, "GetSellPrice", fallback: 0);
        }

        
        var ctx = slotObj.GetComponent<ShopUISlotContext>();
        if (ctx == null) ctx = slotObj.AddComponent<ShopUISlotContext>();
        ctx.isShop = isShop;
        ctx.originalInventorySlot = originalSlot;
        ctx.itemID = itemID;
        ctx.quantity = quantity;
        ctx.price = price;

        
        var shopSlot = slotObj.GetComponent<ShopSlot>();
        if (shopSlot != null)
        {
            shopSlot.SetItem(itemInstance, price);

            
            TrySetFieldOrProperty(shopSlot, "originalSlot", originalSlot);
            TrySetFieldOrProperty(shopSlot, "isShop", isShop);

            //replace dragging with right clicking

            ItemDragHandler dragHandler = itemInstance.GetComponent<ItemDragHandler>();
            if(dragHandler) dragHandler.enabled = false;

            var handler = itemInstance.GetComponent<ShopItemHandler>();
            if (handler == null) handler = itemInstance.AddComponent<ShopItemHandler>();

            handler.Initialize(isShop, originalSlot);
        }
    }
    public void AddItemToShop(int itemID, int quantity)
    {
        if (!currentShop) return;
        currentShop.AddToStock(itemID, quantity);
        RefreshShopDisplay();
    }

    public bool RemoveItemFromShop(int itemID, int quantity)
    {
        if (!currentShop) return false;
        bool success = currentShop.RemoveFromStock(itemID, quantity);
        if (success)
            RefreshShopDisplay();
        return success;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    

    private static int GetIntMember(object obj, string name, int fallback)
    {
        if (obj == null) return fallback;
        var t = obj.GetType();

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj);

        return fallback;
    }

    private static void SetIntMember(object obj, string name, int value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(int))
        {
            f.SetValue(obj, value);
            return;
        }

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(int) && p.CanWrite)
        {
            p.SetValue(obj, value);
        }
    }

    private static void InvokeIfExists(object obj, string methodName)
    {
        if (obj == null) return;
        var t = obj.GetType();
        var m = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m != null && m.GetParameters().Length == 0)
            m.Invoke(obj, null);
    }

    private static int InvokeIntMethod(object obj, string methodName, int fallback)
    {
        if (obj == null) return fallback;
        var t = obj.GetType();
        var m = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m == null) return fallback;

        var parameters = m.GetParameters();
        if (parameters.Length != 0 || m.ReturnType != typeof(int)) return fallback;

        return (int)m.Invoke(obj, null);
    }

    private static void TrySetFieldOrProperty(object obj, string name, object value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && (value == null || f.FieldType.IsInstanceOfType(value)))
        {
            f.SetValue(obj, value);
            return;
        }

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite && (value == null || p.PropertyType.IsInstanceOfType(value)))
        {
            p.SetValue(obj, value);
        }
    }
}


public class ShopUISlotContext : MonoBehaviour
{
    public bool isShop;
    public Slot originalInventorySlot; 
    public int itemID;
    public int quantity;
    public int price;
}