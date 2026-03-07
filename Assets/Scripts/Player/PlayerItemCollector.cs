using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    public static System.Action<Item> OnItemCollected;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Item")) return;

        Item item = collision.GetComponent<Item>();
        if (item == null) return;

        bool itemAdded = inventoryController != null && inventoryController.AddItem(collision.gameObject);
        if (!itemAdded) return;

        OnItemCollected?.Invoke(item);
        item.PickUp();
        Destroy(collision.gameObject);
    }
}
