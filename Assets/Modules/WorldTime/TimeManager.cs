using UnityEngine;
using UnityEngine.Events;

public class TimeManager : MonoBehaviour
{
    [Header("Date & Time Settings")]
    [Range(1, 28)]
    public int dateInMonth = 1;

    [Range(0, 23)]
    public int hour = 7;

    [Range(0, 59)]
    public int minutes = 0;

    private DateTime DateTime;
    public DateTime CurrentDateTime => DateTime;

    [Header("Day Length Settings")]
    [SerializeField] private float realSecondsPerGameDay = 15f;

    private float secondsPerGameMinute;
    private float timer;

    // Old event name (typo) - your ClockManager currently uses this.
    public static UnityAction<DateTime> OnDateimeChanged;

    // New/Correct event name - other scripts might use this.
    public static UnityAction<DateTime> OnDateTimeChanged;

    // Midnight event (00:00)
    public static UnityAction OnSleepTimeReached;

    // Pass-out event (02:00)
    public static UnityAction OnPassOutTime;

    // Minutes since midnight (0..1439)
    public static int CurrentMinutesOfDay { get; private set; }

    public const int MinutesInDay = 1440;

    public const int MidnightMinutes = 0;     // 00:00
    public const int OneAMMinutes = 60;       // 01:00
    public const int PassOutMinutes = 120;    // 02:00
    public const int NewDayStartMinutes = 420; // 07:00

    private bool _sleepTimeTriggered;
    private bool _passOutTriggered;
    private int _lastTotalNumDays;

    private void Awake()
    {
        secondsPerGameMinute = realSecondsPerGameDay / MinutesInDay;
        DateTime = new DateTime(dateInMonth, hour, minutes);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;

        // Avoid instantly firing pass-out if you load into 2:00-6:59.
        _sleepTimeTriggered = CurrentMinutesOfDay == MidnightMinutes;
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;
    }

    private void Start()
    {
        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;
        InvokeDateTimeChanged();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= secondsPerGameMinute)
        {
            timer -= secondsPerGameMinute;

            int prevMinutesOfDay = DateTime.GetMinutesOfDay();
            int prevTotalDays = DateTime.TotalNumDays;

            DateTime.AdvanceMinutes(1);

            CurrentMinutesOfDay = DateTime.GetMinutesOfDay();

            // Reset daily triggers when your DateTime advances the day (at 07:00)
            if (DateTime.TotalNumDays != prevTotalDays || DateTime.TotalNumDays != _lastTotalNumDays)
            {
                _lastTotalNumDays = DateTime.TotalNumDays;
                _sleepTimeTriggered = false;
                _passOutTriggered = false;
            }

            // Midnight (00:00)
            if (!_sleepTimeTriggered && CrossedTime(prevMinutesOfDay, CurrentMinutesOfDay, MidnightMinutes))
            {
                _sleepTimeTriggered = true;
                OnSleepTimeReached?.Invoke();
            }

            // 2:00 AM pass-out
            if (!_passOutTriggered && CrossedTime(prevMinutesOfDay, CurrentMinutesOfDay, PassOutMinutes))
            {
                _passOutTriggered = true;
                OnPassOutTime?.Invoke();
            }

            InvokeDateTimeChanged();
        }
    }

    public void InitializeTime(int startDate, int startHour, int startMinutes)
    {
        DateTime = new DateTime(startDate, startHour, startMinutes);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;

        _sleepTimeTriggered = CurrentMinutesOfDay == MidnightMinutes;
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;

        InvokeDateTimeChanged();
    }

    public void LoadTime(int date, int hour, int minutes, int totalNumDays, int totalNumWeeks)
    {
        DateTime.SetDate(date);
        DateTime.SetTime(hour, minutes);
        DateTime.SetTotals(totalNumDays, totalNumWeeks);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;

        _sleepTimeTriggered = CurrentMinutesOfDay == MidnightMinutes;
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;

        InvokeDateTimeChanged();
    }

    // Your old Sleep() kept
    public void Sleep()
    {
        StartNewDayAt(7, 0);
    }

    // Helper for your sleep/pass-out system
    public void StartNewDayAt(int startHour, int startMinutes)
    {
        DateTime.StartNewDayAt(startHour, startMinutes);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;

        _sleepTimeTriggered = false;
        _passOutTriggered = false;

        InvokeDateTimeChanged();
    }

    private void InvokeDateTimeChanged()
    {
        OnDateimeChanged?.Invoke(DateTime);     // old typo name
        OnDateTimeChanged?.Invoke(DateTime);    // correct name
    }

    // True if target occurred between "from" (exclusive) and "to" (inclusive), handling wrap over midnight
    private bool CrossedTime(int from, int to, int target)
    {
        if (from == to) return false;

        if (from <= to)
            return from < target && target <= to;

        return (from < target && target < MinutesInDay) || (0 <= target && target <= to);
    }
}

[System.Serializable]
public class DateTime
{
    #region Fields
    private Days day;
    private int date;
    private int hour;
    private int minutes;

    private int totalNumDays;
    private int totalNumWeeks;
    #endregion

    #region Properties
    public Days Day => day;
    public int Date => date;
    public int Hour => hour;
    public int Minutes => minutes;

    public int TotalNumDays => totalNumDays;
    public int TotalNumWeeks => totalNumWeeks;
    public int CurrentWeek => totalNumWeeks % 16 == 0 ? 16 : totalNumWeeks % 16;
    #endregion

    #region Constructor
    public DateTime(int date, int hour, int minutes)
    {
        this.date = date;
        this.hour = hour;
        this.minutes = minutes;

        totalNumWeeks = 1 + totalNumDays / 7;

        day = (Days)(date % 7);
        if (day == 0) day = Days.Sunday;
    }
    #endregion

    #region Time Advancement
    public void AdvanceMinutes(int minutesToAdvance)
    {
        minutes += minutesToAdvance;

        while (minutes >= 60)
        {
            minutes -= 60;
            AdvanceHours();
        }
    }

    private void AdvanceHours()
    {
        hour++;

        if (hour >= 24)
            hour = 0;

        // Your design: new day happens at 07:00
        if (hour == 7)
            AdvanceDay();
    }

    private void AdvanceDay()
    {
        date++;
        totalNumDays++;

        if (day == Days.Sunday)
        {
            day = Days.Monday;
            totalNumWeeks++;
        }
        else
        {
            day++;
        }
    }
    #endregion

    public int GetMinutesOfDay() => hour * 60 + minutes;

    public void SetTime(int newHour, int newMinutes)
    {
        hour = Mathf.Clamp(newHour, 0, 23);
        minutes = Mathf.Clamp(newMinutes, 0, 59);
    }

    public void SetDate(int newDate)
    {
        date = newDate;
        day = (Days)(date % 7);
        if (day == 0) day = Days.Sunday;
    }

    public void SetTotals(int totalDays, int totalWeeks)
    {
        totalNumDays = totalDays;
        totalNumWeeks = totalWeeks;
    }

    // Supports hour + minutes
    public void StartNewDayAt(int startHour, int startMinutes)
    {
        AdvanceDay();
        hour = Mathf.Clamp(startHour, 0, 23);
        minutes = Mathf.Clamp(startMinutes, 0, 59);
    }

    // Backwards compatible
    public void StartNewDayAt(int startHour)
    {
        StartNewDayAt(startHour, 0);
    }
}

[System.Serializable]
public enum Days
{
    NULL = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}