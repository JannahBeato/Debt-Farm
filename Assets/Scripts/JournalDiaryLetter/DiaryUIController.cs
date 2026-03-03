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

    private int _index = 0;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (titleText != null) titleText.richText = true; // enables <s></s>
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

        _index = 0;
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
        var entries = JournalManager.Instance != null ? JournalManager.Instance.Entries : null;
        if (entries == null || entries.Count == 0) return;

        _index = Mathf.Min(_index + 1, entries.Count - 1);
        Refresh();
    }

    public void Prev()
    {
        var entries = JournalManager.Instance != null ? JournalManager.Instance.Entries : null;
        if (entries == null || entries.Count == 0) return;

        _index = Mathf.Max(_index - 1, 0);
        Refresh();
    }

    private void Refresh()
    {
        var jm = JournalManager.Instance;
        if (jm == null || jm.Entries.Count == 0)
        {
            if (titleText) titleText.text = "Diary";
            if (bodyText) bodyText.text = "No entries yet.";
            if (pageText) pageText.text = "";
            if (prevButton) prevButton.interactable = false;
            if (nextButton) nextButton.interactable = false;
            return;
        }

        int count = jm.Entries.Count;
        _index = Mathf.Clamp(_index, 0, count - 1);

        var entry = jm.Entries[_index];

        // Page indicator + arrow state
        if (pageText) pageText.text = $"{_index + 1} / {count}";
        if (prevButton) prevButton.interactable = _index > 0;
        if (nextButton) nextButton.interactable = _index < count - 1;

        // Title render
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

        if (bodyText) bodyText.text = string.IsNullOrEmpty(entry.body) ? "" : entry.body;
    }
}