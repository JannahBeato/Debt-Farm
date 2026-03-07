using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntroLetterController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Navigation")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Content")]
    [SerializeField] private string introTitle = "A Letter...";
    [TextArea(10, 30)]
    [SerializeField] private string introBody;

    private SaveController _saveController;

    private const string PAGE_BREAK = "---PAGE---";

    private List<string> _pages = new();
    private int _pageIndex = 0;

    private void Awake()
    {
        _saveController = FindObjectOfType<SaveController>();
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel == null) return;

        if (titleText != null) titleText.text = introTitle;

        BuildPages();
        _pageIndex = 0;

        panel.SetActive(true);
        Time.timeScale = 0f;

        RefreshPage();
    }

    public void Next()
    {
        if (_pages == null || _pages.Count == 0) return;
        _pageIndex = Mathf.Min(_pageIndex + 1, _pages.Count - 1);
        RefreshPage();
    }

    public void Prev()
    {
        if (_pages == null || _pages.Count == 0) return;
        _pageIndex = Mathf.Max(_pageIndex - 1, 0);
        RefreshPage();
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        Time.timeScale = 1f;

        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
            {
                id = "intro-letter",
                title = introTitle,
                body = introBody,
                dayAdded = 1
            });
        }

        if (_saveController != null)
            _saveController.MarkIntroLetterSeenAndSave();

        // Added: reload saved game after closing the intro letter
        if (_saveController != null)
            _saveController.LoadGame();
    }

    private void BuildPages()
    {
        _pages.Clear();

        string text = introBody ?? "";
        text = text.Replace("\r\n", "\n");

        string[] parts = text.Split(new[] { PAGE_BREAK }, System.StringSplitOptions.None);
        foreach (var p in parts)
        {
            _pages.Add(p.Trim('\n', ' '));
        }

        if (_pages.Count == 0) _pages.Add("");
    }

    private void RefreshPage()
    {
        if (bodyText != null)
            bodyText.text = _pages[_pageIndex];

        if (prevButton) prevButton.interactable = _pageIndex > 0;
        if (nextButton) nextButton.interactable = _pageIndex < _pages.Count - 1;
    }
}