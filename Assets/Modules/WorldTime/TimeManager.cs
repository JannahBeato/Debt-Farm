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

    
    public static UnityAction<DateTime> OnDateTimeChanged;

    
    public static UnityAction OnPassOutTime;

    
    public static int CurrentMinutesOfDay { get; private set; }

    
    public const int MinutesInDay = 1440;
    public const int MidnightMinutes = 0;      // 00:00
    public const int PassOutMinutes = 120;     // 02:00
    public const int OneAMMinutes = 60; // 1am 
    public const int NewDayStartMinutes = 420; // 07:00 (your day advances at 7)

    private bool _passOutTriggered;
    private int _lastTotalNumDays;

    private void Awake()
    {
        secondsPerGameMinute = realSecondsPerGameDay / MinutesInDay;
        DateTime = new DateTime(dateInMonth, hour, minutes);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;

        
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;
    }

    private void Start()
    {
        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();
        _lastTotalNumDays = DateTime.TotalNumDays;
        OnDateTimeChanged?.Invoke(DateTime);
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

            
            if (DateTime.TotalNumDays != prevTotalDays || DateTime.TotalNumDays != _lastTotalNumDays)
            {
                _lastTotalNumDays = DateTime.TotalNumDays;
                _passOutTriggered = false;
            }

            
            if (!_passOutTriggered && CrossedTime(prevMinutesOfDay, CurrentMinutesOfDay, PassOutMinutes))
            {
                _passOutTriggered = true;
                OnPassOutTime?.Invoke();
            }

            OnDateTimeChanged?.Invoke(DateTime);
        }
    }

    public void InitializeTime(int startDate, int startHour, int startMinutes)
    {
        DateTime = new DateTime(startDate, startHour, startMinutes);
        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();

        _lastTotalNumDays = DateTime.TotalNumDays;
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;

        OnDateTimeChanged?.Invoke(DateTime);
    }

    public void LoadTime(int date, int hour, int minutes, int totalNumDays, int totalNumWeeks)
    {
        DateTime.SetDate(date);
        DateTime.SetTime(hour, minutes);
        DateTime.SetTotals(totalNumDays, totalNumWeeks);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();

        _lastTotalNumDays = DateTime.TotalNumDays;
        _passOutTriggered = CurrentMinutesOfDay >= PassOutMinutes && CurrentMinutesOfDay < NewDayStartMinutes;

        OnDateTimeChanged?.Invoke(DateTime);
    }


    public void Sleep()
    {
        StartNewDayAt(7, 0);
    }


    public void StartNewDayAt(int startHour, int startMinutes)
    {
        DateTime.StartNewDayAt(startHour, startMinutes);

        CurrentMinutesOfDay = DateTime.GetMinutesOfDay();

        _lastTotalNumDays = DateTime.TotalNumDays;
        _passOutTriggered = false;

        OnDateTimeChanged?.Invoke(DateTime);
    }


    private bool CrossedTime(int from, int to, int target)
    {
        if (from <= to)
        {
            return from < target && target <= to;
        }

        // Wrapped over midnight
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

    #region Bool Checks
    public bool IsNight()
    {
        return hour >= 18 || hour < 6;
    }

    public bool IsMorning()
    {
        return hour >= 6 && hour < 12;
    }

    public bool IsAfternoon()
    {
        return hour >= 12 && hour < 18;
    }
    #endregion

    #region ToStrings
    public override string ToString()
    {
        return $"{day} {date} - {hour:00}:{minutes:00} " +
               $"(Total Days: {totalNumDays} | Total Weeks: {totalNumWeeks})";
    }
    #endregion

    public int GetMinutesOfDay()
    {
        return hour * 60 + minutes;
    }

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


    public void StartNewDayAt(int startHour, int startMinutes)
    {
        AdvanceDay();
        hour = Mathf.Clamp(startHour, 0, 23);
        minutes = Mathf.Clamp(startMinutes, 0, 59);
    }


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
