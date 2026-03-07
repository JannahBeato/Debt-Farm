using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    private ItemDictionary itemDictionary;

    [Header("UI")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;

    [Header("Legacy/Optional")]
    public GameObject[] itemPrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        itemDictionary = FindObjectOfType<ItemDictionary>();
        if (itemDictionary == null)
            Debug.LogError("InventoryController: No ItemDictionary found in the scene!");
    }

    private void Start()
    {
        EnsureSlotsExist();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void EnsureSlotsExist()
    {
        if (inventoryPanel == null || slotPrefab == null) return;

        int existingInventorySlots = 0;

        foreach (Transform child in inventoryPanel.transform)
        {
            if (child.GetComponent<TrashSlot>() != null)
                continue;

            existingInventorySlots++;
        }

        if (slotCount <= 0) slotCount = existingInventorySlots;
        if (slotCount <= 0) return;

        while (existingInventorySlots < slotCount)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
            existingInventorySlots++;
        }

        foreach (Transform child in inventoryPanel.transform)
        {
            if (child.GetComponent<TrashSlot>() != null)
                continue;

            if (child.GetComponent<Slot>() == null)
                child.gameObject.AddComponent<Slot>();
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryController.AddItem: inventoryPanel is not assigned.");
            return false;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("InventoryController.AddItem: itemPrefab was null.");
            return false;
        }

        EnsureSlotsExist();

        Item prefabItem = itemPrefab.GetComponent<Item>();
        if (prefabItem == null)
        {
            Debug.LogWarning("InventoryController.AddItem: prefab has no Item component.");
            return false;
        }

        int prefabID = prefabItem.ID;
        int amountToAdd = Mathf.Max(1, prefabItem.quantity);

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item existing = slot.currentItem.GetComponent<Item>();
            if (existing != null && existing.ID == prefabID)
            {
                existing.AddToStack(amountToAdd);
                return true;
            }
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);

                RectTransform rt = newItem.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localScale = Vector3.one;

                Item newItemComp = newItem.GetComponent<Item>();
                if (newItemComp != null)
                {
                    newItemComp.quantity = amountToAdd;
                    newItemComp.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public void RebuildItemCounts()
    {
        if (inventoryPanel == null) return;

        EnsureSlotsExist();
        Dictionary<int, Slot> firstSlotById = new Dictionary<int, Slot>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;

            int id = item.ID;

            if (!firstSlotById.ContainsKey(id))
            {
                firstSlotById[id] = slot;
                item.UpdateQuantityDisplay();
                continue;
            }

            Slot firstSlot = firstSlotById[id];
            Item firstItem = firstSlot.currentItem != null ? firstSlot.currentItem.GetComponent<Item>() : null;

            if (firstItem == null)
            {
                firstSlotById[id] = slot;
                item.UpdateQuantityDisplay();
                continue;
            }

            int addQty = Mathf.Max(1, item.quantity);
            firstItem.quantity += addQty;
            firstItem.UpdateQuantityDisplay();

            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        var invData = new List<InventorySaveData>();
        if (inventoryPanel == null) return invData;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;

            invData.Add(new InventorySaveData
            {
                itemID = item.ID,
                slotIndex = slotTransform.GetSiblingIndex(),
                quantity = Mathf.Max(1, item.quantity)
            });
        }

        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> saveData)
    {
        if (inventoryPanel == null) return;

        if (itemDictionary == null)
            itemDictionary = FindObjectOfType<ItemDictionary>();

        if (saveData == null) saveData = new List<InventorySaveData>();

        EnsureSlotsExist();

        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            var slot = inventoryPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot == null) continue;

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        for (int i = 0; i < saveData.Count; i++)
        {
            var data = saveData[i];

            if (data.slotIndex < 0 || data.slotIndex >= inventoryPanel.transform.childCount)
                continue;

            var slotTransform = inventoryPanel.transform.GetChild(data.slotIndex);
            var slot = slotTransform.GetComponent<Slot>();
            if (slot == null) continue;

            if (itemDictionary == null) continue;

            var prefab = itemDictionary.GetItemPrefab(data.itemID);
            if (prefab == null) continue;

            var itemObj = Instantiate(prefab, slotTransform);

            RectTransform rt = itemObj.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = Vector2.zero;
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;

            slot.currentItem = itemObj;

            var item = itemObj.GetComponent<Item>();
            if (item != null)
            {
                item.quantity = Mathf.Max(1, data.quantity);
                item.UpdateQuantityDisplay();
            }
        }

        RebuildItemCounts();
    }

    public bool AddItemById(int itemId)
    {
        if (itemDictionary == null) itemDictionary = FindObjectOfType<ItemDictionary>();
        if (itemDictionary == null)
        {
            Debug.LogError("InventoryController: ItemDictionary missing.");
            return false;
        }

        GameObject uiPrefab = itemDictionary.GetItemPrefab(itemId);
        if (uiPrefab == null) return false;

        return AddItem(uiPrefab);
    }
}
