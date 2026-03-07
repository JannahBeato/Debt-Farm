using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [Header("Optional Visual Feedback")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.5f, 0.5f, 1f);

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        SetNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormal();
    }

    public void OnDrop(PointerEventData eventData)
    {
        SetNormal();
    }

    public void SetNormal()
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }

    public void SetHover()
    {
        if (targetImage != null)
            targetImage.color = hoverColor;
    }
}