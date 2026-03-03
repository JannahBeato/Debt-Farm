using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiaryUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Navigation UI (optional)")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text pageText; // e.g. "2 / 7"

    [Header("Objective Styling")]
    [SerializeField] private Color objectiveIncompleteColor = Color.black;
    [SerializeField] private Color objectiveCompleteColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private const string PAGE_BREAK = "---PAGE---";

    private int _entryIndex = 0;
    private int _pageIndex = 0;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (titleText != null) titleText.richText = true;
        if (bodyText != null) bodyText.richText = true;
    }

    public void Toggle()
    {
        if (panel == null) return;

        bool open = !panel.activeSelf;
        if (open) Open();
        else Close();
    }

    public void Open()
    {
        if (panel == null) return;

        panel.SetActive(true);
        Time.timeScale = 0f;

        _entryIndex = 0;
        _pageIndex = 0;
        Refresh();
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Next()
    {
        var jm = JournalManager.Instance;
        if (jm == null || jm.Entries.Count == 0) return;

        var entry = jm.Entries[_entryIndex];
        var pages = GetPages(entry);

        if (_pageIndex < pages.Length - 1)
        {
            _pageIndex++;
            Refresh();
            return;
        }

        _entryIndex = Mathf.Min(_entryIndex + 1, jm.Entries.Count - 1);
        _pageIndex = 0;
        Refresh();
    }

    public void Prev()
    {
        var jm = JournalManager.Instance;
        if (jm == null || jm.Entries.Count == 0) return;

        if (_pageIndex > 0)
        {
            _pageIndex--;
            Refresh();
            return;
        }

        if (_entryIndex > 0)
        {
            _entryIndex--;
            var entry = jm.Entries[_entryIndex];
            var pages = GetPages(entry);
            _pageIndex = Mathf.Max(0, pages.Length - 1);
            Refresh();
        }
    }

    private void Refresh()
    {
        var jm = JournalManager.Instance;
        if (jm == null || jm.Entries.Count == 0)
        {
            if (titleText) titleText.text = "Diary";
            if (bodyText) bodyText.text = "No entries yet.";
            if (pageText) pageText.text = "";
            SetButtonState(prevButton, false); // Added
            SetButtonState(nextButton, false); // Added
            return;
        }

        int entryCount = jm.Entries.Count;
        _entryIndex = Mathf.Clamp(_entryIndex, 0, entryCount - 1);

        var entry = jm.Entries[_entryIndex];
        var pages = GetPages(entry);
        _pageIndex = Mathf.Clamp(_pageIndex, 0, Mathf.Max(0, pages.Length - 1));

        if (pageText)
        {
            string pagePart = pages.Length > 1 ? $" (Page {_pageIndex + 1}/{pages.Length})" : "";
            pageText.text = $"{_entryIndex + 1} / {entryCount}{pagePart}";
        }

        bool canPrev = _pageIndex > 0 || _entryIndex > 0;
        bool canNext = (_pageIndex < pages.Length - 1) || (_entryIndex < entryCount - 1);

        SetButtonState(prevButton, canPrev); // Added
        SetButtonState(nextButton, canNext); // Added

        if (entry.isObjective)
        {
            if (entry.completed)
            {
                if (titleText)
                {
                    titleText.color = objectiveCompleteColor;
                    titleText.text = $"<s>{entry.title}</s>";
                }
            }
            else
            {
                if (titleText)
                {
                    titleText.color = objectiveIncompleteColor;
                    titleText.text = entry.title;
                }
            }
        }
        else
        {
            if (titleText)
            {
                titleText.color = Color.black;
                titleText.text = entry.title;
            }
        }

        if (bodyText) bodyText.text = pages.Length > 0 ? pages[_pageIndex] : "";
    }

    private string[] GetPages(JournalEntrySaveData entry)
    {
        string body = entry != null && !string.IsNullOrEmpty(entry.body) ? entry.body : "";
        body = body.Replace("\r\n", "\n");

        var parts = body.Split(new[] { PAGE_BREAK }, System.StringSplitOptions.None);
        for (int i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim('\n', ' ');

        if (parts.Length == 0) return new[] { "" };
        if (parts.Length == 1 && string.IsNullOrEmpty(parts[0])) return new[] { "" };

        return parts;
    }

    private static void SetButtonState(Button button, bool enabled) // Added
    {
        if (button == null) return;

        button.interactable = enabled;

        var img = button.image;
        if (img != null)
        {
            var c = img.color;
            c.a = enabled ? 1f : 0.35f;
            img.color = c;
        }
    }
}