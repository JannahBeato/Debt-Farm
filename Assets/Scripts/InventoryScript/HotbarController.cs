using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [Header("UI")]
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10; // 1-0 on keyboard

    [Header("Highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private int defaultSelectedIndex = 0;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;
    private int selectedIndex;

    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }

        selectedIndex = Mathf.Clamp(defaultSelectedIndex, 0, Mathf.Max(0, slotCount - 1));
    }

    private void Start()
    {
        EnsureSlotsExist();

        if (hotbarPanel != null && hotbarPanel.transform.childCount > 0)
            SelectSlotByIndex(selectedIndex);
    }

    private void Update()
    {
        if (hotbarPanel == null) return;

        for (int i = 0; i < Mathf.Min(slotCount, hotbarKeys.Length); i++)
        {
            if (Keyboard.current != null && Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlotByIndex(i);
            }
        }
    }

    private void EnsureSlotsExist()
    {
        if (hotbarPanel == null || slotPrefab == null) return;

        while (hotbarPanel.transform.childCount < slotCount)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            if (slotTransform.GetComponent<Slot>() == null)
                slotTransform.gameObject.AddComponent<Slot>();
        }
    }

    public void SelectSlotByIndex(int index)
    {
        if (hotbarPanel == null || hotbarPanel.transform.childCount == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, hotbarPanel.transform.childCount - 1);

        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Transform slotTransform = hotbarPanel.transform.GetChild(i);
            Image img = slotTransform.GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }
    }

    public void SelectSlotBySlot(Slot slot)
    {
        if (slot == null || hotbarPanel == null)
            return;

        if (slot.transform.parent != hotbarPanel.transform)
            return;

        SelectSlotByIndex(slot.transform.GetSiblingIndex());
    }

    public void UseItemInSlot(int index)
    {
        if (hotbarPanel == null || index < 0 || index >= hotbarPanel.transform.childCount)
            return;

        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot != null && slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null)
                item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();

        if (hotbarPanel == null) return hotbarData;

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;

            hotbarData.Add(new InventorySaveData
            {
                itemID = item.ID,
                slotIndex = slotTransform.GetSiblingIndex(),
                quantity = Mathf.Max(1, item.quantity)
            });
        }

        return hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> saveData)
    {
        if (hotbarPanel == null) return;
        if (saveData == null) saveData = new List<InventorySaveData>();
        if (itemDictionary == null) itemDictionary = FindObjectOfType<ItemDictionary>();

        EnsureSlotsExist();

        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Transform slotTransform = hotbarPanel.transform.GetChild(i);
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null) slot = slotTransform.gameObject.AddComponent<Slot>();

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        foreach (var data in saveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= slotCount) continue;

            Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            if (slot == null) continue;

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null) continue;

            GameObject itemObj = Instantiate(itemPrefab, slot.transform);

            RectTransform rt = itemObj.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = Vector2.zero;
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;

            Item item = itemObj.GetComponent<Item>();
            if (item != null)
            {
                item.quantity = Mathf.Max(1, data.quantity);
                item.UpdateQuantityDisplay();
            }

            slot.currentItem = itemObj;
        }

        SelectSlotByIndex(selectedIndex);
    }

    public int GetSelectedIndex() => selectedIndex;

    public Item GetSelectedItem()
    {
        if (hotbarPanel == null || hotbarPanel.transform.childCount <= selectedIndex) return null;

        Slot slot = hotbarPanel.transform.GetChild(selectedIndex).GetComponent<Slot>();
        if (slot == null || slot.currentItem == null) return null;

        return slot.currentItem.GetComponent<Item>();
    }

    public GameObject GetSelectedItemObject()
    {
        if (hotbarPanel == null || hotbarPanel.transform.childCount <= selectedIndex) return null;

        Slot slot = hotbarPanel.transform.GetChild(selectedIndex).GetComponent<Slot>();
        if (slot == null) return null;

        return slot.currentItem;
    }

    public bool ConsumeSelectedItem()
    {
        if (hotbarPanel == null || hotbarPanel.transform.childCount <= selectedIndex) return false;

        Slot slot = hotbarPanel.transform.GetChild(selectedIndex).GetComponent<Slot>();
        if (slot == null || slot.currentItem == null) return false;

        Item item = slot.currentItem.GetComponent<Item>();
        if (item == null)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;
            return true;
        }

        if (item.quantity > 1)
        {
            item.quantity -= 1;
            item.UpdateQuantityDisplay();
            return true;
        }

        Destroy(slot.currentItem);
        slot.currentItem = null;
        return true;
    }
}
