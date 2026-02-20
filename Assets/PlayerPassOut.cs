using System.Collections;
using UnityEngine;

public class PlayerPassOut : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerEnergy _energy;

    [Header("End of Day")]
    [SerializeField] private float _endDayDelaySeconds = 0.75f;

    [Header("New Day Start Time")]
    [SerializeField] private int _newDayStartHour = 7;
    [SerializeField] private int _newDayStartMinutes = 0;

    [Header("Wake Energy Rules")]
    [Tooltip("Sleep before 01:00 -> 100% (includes 07:00-23:59 and 00:00-00:59)")]
    [SerializeField] private float _wakeEnergyIfSleepBefore1AM = 1.00f;

    [Tooltip("Sleep 01:00-01:59 -> 80%")]
    [SerializeField] private float _wakeEnergyIfSleepBetween1And2 = 0.80f;

    [Tooltip("Sleep at/after 02:00 OR pass out at 02:00 -> 20%")]
    [SerializeField] private float _wakeEnergyIfAfter2OrPassOut = 0.20f;

    [Header("Debug")]
    [SerializeField] private bool _debugLog;

    private bool _endingDay;

    private void Awake()
    {
        if (_movement == null) _movement = GetComponent<PlayerMovement>();
        if (_energy == null) _energy = GetComponent<PlayerEnergy>();
        if (_timeManager == null) _timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void OnEnable()
    {
        TimeManager.OnPassOutTime += HandlePassOutTime;
    }

    private void OnDisable()
    {
        TimeManager.OnPassOutTime -= HandlePassOutTime;
    }

    private void HandlePassOutTime()
    {
        if (_endingDay) return;
        StartCoroutine(EndDayRoutine(EndDayReason.PassOut, sleptAtMinutesOfDay: -1));
    }

    public void SleepNow()
    {
        if (_endingDay) return;

        // IMPORTANT: read time from the referenced TimeManager instance, not the static.
        int sleptAt = (_timeManager != null)
            ? _timeManager.CurrentDateTime.GetMinutesOfDay()
            : TimeManager.CurrentMinutesOfDay;

        StartCoroutine(EndDayRoutine(EndDayReason.Slept, sleptAt));
    }

    private IEnumerator EndDayRoutine(EndDayReason reason, int sleptAtMinutesOfDay)
    {
        _endingDay = true;

        if (_movement != null)
            _movement.SetCanMove(false);

        // Realtime so this finishes even if another script sets Time.timeScale = 0
        yield return new WaitForSecondsRealtime(_endDayDelaySeconds);

        // Decide energy BEFORE changing time
        float wakePercent = GetWakeEnergyPercent(reason, sleptAtMinutesOfDay);

        // Advance day FIRST (prevents other "day start" listeners from overwriting our final energy)
        if (_timeManager != null)
            _timeManager.StartNewDayAt(_newDayStartHour, _newDayStartMinutes);
        else
            Debug.LogError("PlayerPassOut: TimeManager reference is missing. Assign it in the Inspector.");

        // Apply energy last
        if (_energy != null)
            _energy.RestorePercent(wakePercent);

        if (_movement != null)
            _movement.SetCanMove(true);

        _endingDay = false;
    }

    private float GetWakeEnergyPercent(EndDayReason reason, int sleptAtMinutesOfDay)
    {
        if (reason == EndDayReason.PassOut || sleptAtMinutesOfDay < 0)
        {
            if (_debugLog) Debug.Log("EndDay: PASS OUT -> 20%");
            return _wakeEnergyIfAfter2OrPassOut;
        }

        int h = sleptAtMinutesOfDay / 60;
        int m = sleptAtMinutesOfDay % 60;

        float result;

        // 07:00-23:59 is "before 1 AM" for the coming night
        if (h >= 7)
            result = _wakeEnergyIfSleepBefore1AM;          // 100%
        else if (h == 0)
            result = _wakeEnergyIfSleepBefore1AM;          // 100%
        else if (h == 1)
            result = _wakeEnergyIfSleepBetween1And2;       // 80%
        else
            result = _wakeEnergyIfAfter2OrPassOut;         // 20% for 02:00-06:59

        if (_debugLog)
            Debug.Log("EndDay: Slept at " + h.ToString("00") + ":" + m.ToString("00") +
                      " -> wakePercent=" + (result * 100f).ToString("0") + "%");

        return result;
    }

    private enum EndDayReason
    {
        Slept,
        PassOut
    }
}