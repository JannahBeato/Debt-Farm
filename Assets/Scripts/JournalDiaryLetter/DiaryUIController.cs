using UnityEngine;
using TMPro;

public class DiaryUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private int _index = 0;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
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
        if (JournalManager.Instance == null || JournalManager.Instance.Entries.Count == 0)
        {
            if (titleText) titleText.text = "Diary";
            if (bodyText) bodyText.text = "No entries yet.";
            return;
        }

        _index = Mathf.Clamp(_index, 0, JournalManager.Instance.Entries.Count - 1);

        var entry = JournalManager.Instance.Entries[_index];
        string prefix = entry.isObjective ? (entry.completed ? "[X] " : "[ ] ") : "";

        if (titleText) titleText.text = prefix + entry.title;
        if (bodyText) bodyText.text = string.IsNullOrEmpty(entry.body) ? "" : entry.body;
    }
}