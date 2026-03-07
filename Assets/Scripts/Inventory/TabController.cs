using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNo)
    {
        if (pages == null || tabImages == null) return;
        if (pages.Length == 0 || tabImages.Length == 0) return;

        tabNo = Mathf.Clamp(tabNo, 0, Mathf.Min(pages.Length, tabImages.Length) - 1);

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null) pages[i].SetActive(false);

            if (i < tabImages.Length && tabImages[i] != null)
                tabImages[i].color = Color.grey;
        }

        if (pages[tabNo] != null) pages[tabNo].SetActive(true);
        if (tabImages[tabNo] != null) tabImages[tabNo].color = Color.white;
    }
}
