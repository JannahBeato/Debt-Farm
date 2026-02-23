using UnityEngine;
using System;

public class DayCycleController : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;

    public event Action SleepTimeReached;
    public event Action PassOutTimeReached;

    private bool sleepTriggered;
    private bool passOutTriggered;
    private int lastTotalDays;

    private bool initialized;

    private void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void OnEnable()
    {
        TimeManager.OnDateimeChanged += OnTimeChanged; // keep typo event if that's what you use
    }

    private void OnDisable()
    {
        TimeManager.OnDateimeChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(DateTime dt)
    {
        int minutes = dt.GetMinutesOfDay();

        // ✅ FIRST UPDATE: only sync state, DO NOT fire events
        if (!initialized)
        {
            initialized = true;
            lastTotalDays = dt.TotalNumDays;

            sleepTriggered = (minutes == TimeManager.MidnightMinutes);
            passOutTriggered = (minutes >= TimeManager.PassOutMinutes && minutes < TimeManager.NewDayStartMinutes);

            return;
        }

        // reset per-day triggers when a new day starts
        if (dt.TotalNumDays != lastTotalDays)
        {
            lastTotalDays = dt.TotalNumDays;
            sleepTriggered = false;
            passOutTriggered = false;
        }

        if (!sleepTriggered && minutes == TimeManager.MidnightMinutes)
        {
            sleepTriggered = true;
            SleepTimeReached?.Invoke();
        }

        if (!passOutTriggered && minutes == TimeManager.PassOutMinutes)
        {
            passOutTriggered = true;
            PassOutTimeReached?.Invoke();
        }
    }
}