using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    void Start()
    {
        if (inventoryPanel == null || slotPrefab == null) return;

        for (int i = 0; i < slotCount; i++)
        {
            // Instantiate slot
            GameObject slotObj = Instantiate(slotPrefab, inventoryPanel.transform);


            Slot slot = slotObj.GetComponent<Slot>();
            if (slot == null)
            {
                Debug.LogError("Slot prefab is missing the Slot component!");
                continue;
            }

            // Put an item into the slot if we have one
            if (itemPrefabs != null && i < itemPrefabs.Length && itemPrefabs[i] != null)
            {
                GameObject item = Instantiate(itemPrefabs[i], slotObj.transform);

                RectTransform itemRT = item.GetComponent<RectTransform>();
                if (itemRT != null)
                    itemRT.anchoredPosition = Vector2.zero;

                slot.currentItem = item;
            }
        }
    }
}
