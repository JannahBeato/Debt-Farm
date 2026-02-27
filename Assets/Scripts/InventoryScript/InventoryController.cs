using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    // Start is called before the first frame update
    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

    }

    public bool AddItem(GameObject itemPrefab)
    {
        //Look for empty slot
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTranform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTranform.GetSiblingIndex() });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> saveData)
    {
        if (inventoryPanel == null) return;
        if (saveData == null) saveData = new List<InventorySaveData>();

        // If slotCount wasn't set in Inspector, infer from existing slots
        if (slotCount <= 0) slotCount = inventoryPanel.transform.childCount;

        // Ensure we have slots
        while (inventoryPanel.transform.childCount < slotCount)
            Instantiate(slotPrefab, inventoryPanel.transform);

        // Clear items only (keep slots)
        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            var slot = inventoryPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot == null) continue;

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        // Populate
        foreach (var data in saveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= slotCount) continue;

            var slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            if (slot == null) continue;

            var itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null) continue;

            var item = Instantiate(itemPrefab, slot.transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            slot.currentItem = item;
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