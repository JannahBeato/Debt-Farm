using UnityEngine;

public class SleepUIController : MonoBehaviour
{
    private enum PendingEndDay { None, Sleep, PassOut }
    private PendingEndDay _pending = PendingEndDay.None;

    [SerializeField] private DayCycleController dayCycle;
    [SerializeField] private EndOfDayService endOfDay;
    [SerializeField] private FadeUI fadeUI;
    [SerializeField] private GameObject nextDayButton;

    private void Awake()
    {
        if (dayCycle == null) dayCycle = FindFirstObjectByType<DayCycleController>();
        if (endOfDay == null) endOfDay = FindFirstObjectByType<EndOfDayService>();
    }

    private void OnEnable()
    {
        if (dayCycle != null)
            dayCycle.PassOutTimeReached += OnPassOutTime;

        if (endOfDay != null)
        {
            endOfDay.EndOfDayStarted += OnEndDayStarted;
            endOfDay.EndOfDayCompleted += OnEndDayCompleted;
        }
    }

    private void OnDisable()
    {
        if (dayCycle != null)
            dayCycle.PassOutTimeReached -= OnPassOutTime;

        if (endOfDay != null)
        {
            endOfDay.EndOfDayStarted -= OnEndDayStarted;
            endOfDay.EndOfDayCompleted -= OnEndDayCompleted;
        }
    }

    // ✅ Call this from bed "Yes"
    public void ShowSleepUI()
    {
        _pending = PendingEndDay.Sleep;
        ShowEndOfDayUI();
    }

    private void OnPassOutTime()
    {
        _pending = PendingEndDay.PassOut;
        ShowEndOfDayUI();
    }

    private void ShowEndOfDayUI()
    {
        // Fade first (recommended). If FadeUI uses scaled time, make FadeUI use WaitForSecondsRealtime.
        if (fadeUI != null) fadeUI.FadeToBlack();

        Time.timeScale = 0f;

        if (nextDayButton != null) nextDayButton.SetActive(true);
    }

    public void OnNextDayButtonPressed()
    {
        if (nextDayButton != null) nextDayButton.SetActive(false);

        Time.timeScale = 1f;

        if (endOfDay == null) return;

        if (_pending == PendingEndDay.PassOut) endOfDay.PassOutNow();
        else if (_pending == PendingEndDay.Sleep) endOfDay.SleepNow();

        _pending = PendingEndDay.None;
    }

    private void OnEndDayStarted()
    {
        if (fadeUI != null) fadeUI.FadeToBlack();
    }

    private void OnEndDayCompleted()
    {
        if (fadeUI != null) fadeUI.FadeFromBlack();
    }
}