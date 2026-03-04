using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;

    [Min(1)]
    public int quantity = 1;

    [Header("UI (Optional)")]
    [SerializeField] private TMP_Text quantityText;

    public int buyPrice = 10;
    [Range(0, 1)]
    public float sellPriceMultiplier = 0.5f;

    private void Awake()
    {
        CacheQuantityText();

       
        if (quantityText == null)
            TryAutoCreateQuantityText();

        UpdateQuantityDisplay();
    }

    private void CacheQuantityText()
    {
        if (quantityText != null) return;

        // Prefer a known child name if you have one in some prefabs
        Transform t =
            transform.Find("QuantityText") ??
            transform.Find("QtyText") ??
            transform.Find("Quantity") ??
            transform.Find("Qty");

        if (t != null)
            quantityText = t.GetComponent<TMP_Text>();

        // Otherwise try to find any TMP child whose name suggests quantity
        if (quantityText == null)
        {
            foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
            {
                if (txt == null) continue;
                string n = txt.gameObject.name.ToLowerInvariant();
                if (n.Contains("qty") || n.Contains("quantity"))
                {
                    quantityText = txt;
                    break;
                }
            }
        }
    }

    private void TryAutoCreateQuantityText()
    {
        // Only auto-create for UI items
        if (GetComponent<RectTransform>() == null) return;

        var go = new GameObject("QuantityText", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-6f, -6f);   // top-right inset
        rt.sizeDelta = new Vector2(64f, 32f);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.TopRight;
        tmp.fontSize = 24;
        tmp.text = "";

        quantityText = tmp;
    }

    public int GetSellPrice() => Mathf.RoundToInt(buyPrice * sellPriceMultiplier);

    public void UpdateQuantityDisplay()
    {
        if (quantityText == null) return;

        if (!quantityText.gameObject.activeSelf)
            quantityText.gameObject.SetActive(true);

        // show only when > 1 (same behavior as your desired UI)
        quantityText.text = quantity > 1 ? quantity.ToString() : "";

        // keep it drawn over the icon
        quantityText.transform.SetAsLastSibling();
    }

    public void AddToStack(int amount = 1)
    {
        quantity += Mathf.Max(1, amount);
        UpdateQuantityDisplay();
    }

    public void RemoveFromStack(int amount = 1)
    {
        quantity -= Mathf.Max(1, amount);
        if (quantity < 1) quantity = 1;
        UpdateQuantityDisplay();
    }

    public virtual void UseItem()
    {
        Debug.Log($"Using item: {Name}");
    }

    public virtual void PickUp()
    {
        Sprite itemIcon = GetComponent<Image>() != null ? GetComponent<Image>().sprite : null;
        if (ItemPickupUIController.Instance != null)
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
    }
}