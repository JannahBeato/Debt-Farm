using UnityEngine;
using TMPro;

public class IntroLetterController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Content")]
    [SerializeField] private string introTitle = "A Letter...";
    [TextArea(10, 30)]
    [SerializeField] private string introBody;

    private SaveController _saveController;

    private void Awake()
    {
        _saveController = FindObjectOfType<SaveController>();
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel == null) return;

        if (titleText != null) titleText.text = introTitle;
        if (bodyText != null) bodyText.text = introBody;

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        Time.timeScale = 1f;

        // Store into journal
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

        // Mark seen + save
        if (_saveController != null)
            _saveController.MarkIntroLetterSeenAndSave();
    }
}