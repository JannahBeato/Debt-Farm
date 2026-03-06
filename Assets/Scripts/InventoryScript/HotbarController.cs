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
        if (hotbarPanel != null && hotbarPanel.transform.childCount > 0)
            SelectSlotByIndex(selectedIndex);
    }

    private void Update()
    {
        if (hotbarPanel == null) return;

        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current != null && Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlotByIndex(i);
            }
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
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    hotbarData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex()
                    });
                }
            }
        }

        return hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> saveData)
    {
        if (hotbarPanel == null) return;
        if (saveData == null) saveData = new List<InventorySaveData>();

        while (hotbarPanel.transform.childCount < slotCount)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

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

            GameObject item = Instantiate(itemPrefab, slot.transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            slot.currentItem = item;
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

        Destroy(slot.currentItem);
        slot.currentItem = null;
        return true;
    }
}