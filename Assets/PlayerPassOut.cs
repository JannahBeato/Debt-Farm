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
    [Tooltip("Sleep at 12:00 AM (midnight) or before -> 100%")]
    [SerializeField] private float _wakeEnergyIfSleepBeforeOrAtMidnight = 1.00f;

    [Tooltip("Sleep after midnight but before 2:00 AM -> 80%")]
    [SerializeField] private float _wakeEnergyIfSleepAfterMidnight = 0.80f;

    [Tooltip("Don't sleep, pass out at 2:00 AM -> 20%")]
    [SerializeField] private float _wakeEnergyIfPassOut = 0.20f;

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

        int sleptAt = TimeManager.CurrentMinutesOfDay;
        StartCoroutine(EndDayRoutine(EndDayReason.Slept, sleptAt));
    }

    private IEnumerator EndDayRoutine(EndDayReason reason, int sleptAtMinutesOfDay)
    {
        _endingDay = true;


        if (_movement != null)
            _movement.SetCanMove(false);


        yield return new WaitForSeconds(_endDayDelaySeconds);


        float wakePercent = GetWakeEnergyPercent(reason, sleptAtMinutesOfDay);
        if (_energy != null)
            _energy.RestorePercent(wakePercent);


        if (_timeManager != null)
            _timeManager.StartNewDayAt(_newDayStartHour, _newDayStartMinutes);
        else
            Debug.LogError("PlayerPassOut: TimeManager reference is missing. Assign it in the Inspector.");


        if (_movement != null)
            _movement.SetCanMove(true);

        _endingDay = false;
    }

    private float GetWakeEnergyPercent(EndDayReason reason, int sleptAtMinutesOfDay)
    {
        if (reason == EndDayReason.PassOut)
            return _wakeEnergyIfPassOut;


        if (sleptAtMinutesOfDay == TimeManager.MidnightMinutes)
            return _wakeEnergyIfSleepBeforeOrAtMidnight;

        if (sleptAtMinutesOfDay > TimeManager.MidnightMinutes &&
            sleptAtMinutesOfDay < TimeManager.PassOutMinutes)
            return _wakeEnergyIfSleepAfterMidnight;

        
        return _wakeEnergyIfSleepBeforeOrAtMidnight;
    }

    private enum EndDayReason
    {
        Slept,
        PassOut
    }
}
