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
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache dictionary
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

        // If slotCount not set, infer from existing children
        if (slotCount <= 0) slotCount = inventoryPanel.transform.childCount;

        // If still 0, do nothing (user likely forgot to set slotCount)
        if (slotCount <= 0) return;

        // Create missing slots
        while (inventoryPanel.transform.childCount < slotCount)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // Ensure each slot has a Slot component
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (slotTransform.GetComponent<Slot>() == null)
                slotTransform.gameObject.AddComponent<Slot>();
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

        // Look for empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);

                // UI placement
                RectTransform rt = newItem.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;
                newItem.transform.localPosition = Vector3.zero;

                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
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
                slotIndex = slotTransform.GetSiblingIndex()
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

        // Clear items only (keep slots)
        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            var slot = inventoryPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot == null) continue;

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        // Populate
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

            slot.currentItem = itemObj;
        }
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