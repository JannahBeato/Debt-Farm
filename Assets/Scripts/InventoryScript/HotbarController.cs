using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [Header("UI")]
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10; // 1-0 on the keyboard

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

        // Hotbar keys based on slot count (1..9 then 0)
        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }

        selectedIndex = Mathf.Clamp(defaultSelectedIndex, 0, slotCount - 1);
    }

    private void Start()
    {
        // If your slots already exist in the scene, highlight the default one
        if (hotbarPanel != null && hotbarPanel.transform.childCount >= slotCount)
            SelectSlot(selectedIndex);
    }

    void Update()
    {
        // Check for key presses -> select slot
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current != null && Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);

            }
        }
    }

    void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, slotCount - 1);

        // Color the Image component on each slot root (your prefab has Image)
        for (int i = 0; i < slotCount; i++)
        {
            Transform slotTransform = hotbarPanel.transform.GetChild(i);
            Image img = slotTransform.GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }
    }

    void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                hotbarData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex()
                });
            }
        }
        return hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> saveData)
    {
        if (hotbarPanel == null) return;
        if (saveData == null) saveData = new List<InventorySaveData>();

        // Ensure we have exactly slotCount slots
        while (hotbarPanel.transform.childCount < slotCount)
            Instantiate(slotPrefab, hotbarPanel.transform);

        // Clear items only (keep slots)
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            var slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot == null) continue;

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;
        }

        // Populate
        foreach (var data in saveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= slotCount) continue;

            var slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            if (slot == null) continue;

            var itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null) continue;

            var item = Instantiate(itemPrefab, slot.transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            slot.currentItem = item;
        }

        SelectSlot(selectedIndex);
    }

    // Optional helper if other scripts want the selected slot
    public int GetSelectedIndex() => selectedIndex;

    // Optional helper to get selected item
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