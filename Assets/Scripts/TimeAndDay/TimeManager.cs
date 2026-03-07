using UnityEngine;
using UnityEngine.Events;

public class TimeManager : MonoBehaviour
{
    [Header("Date & Time Settings")]
    [Range(1, 28)] public int dateInMonth = 1;
    [Range(0, 23)] public int hour = 7;
    [Range(0, 59)] public int minutes = 0;

    private DateTime _dateTime;
    public DateTime CurrentDateTime => _dateTime;

    [Header("Day Length Settings")]
    [SerializeField] private float realSecondsPerGameDay = 15f;

    private float secondsPerGameMinute;
    private float timer;

    public static UnityAction<DateTime> OnDateimeChanged;
    public static UnityAction<DateTime> OnDateTimeChanged;

    public static int CurrentMinutesOfDay { get; private set; }

    public const int MinutesInDay = 1440;
    public const int MidnightMinutes = 0;
    public const int OneAMMinutes = 60;
    public const int PassOutMinutes = 120;
    public const int NewDayStartMinutes = 420;

    private void Reset()
    {
        dateInMonth = 1;
        hour = 7;
        minutes = 0;
        realSecondsPerGameDay = 15f;
    }

    private void Awake()
    {
        secondsPerGameMinute = realSecondsPerGameDay / MinutesInDay;
        _dateTime = new DateTime(dateInMonth, hour, minutes);
        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();
    }

    private void Start()
    {
        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();
        InvokeDateTimeChanged();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < secondsPerGameMinute)
            return;

        timer -= secondsPerGameMinute;

        _dateTime.AdvanceMinutes(1);
        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();

        InvokeDateTimeChanged();
    }

    public void InitializeTime(int startDate, int startHour, int startMinutes)
    {
        _dateTime = new DateTime(startDate, startHour, startMinutes);
        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();
        timer = 0f;
        InvokeDateTimeChanged();
    }

    public void LoadTime(int date, int hour, int minutes, int totalNumDays, int totalNumWeeks)
    {
        _dateTime.SetDate(date);
        _dateTime.SetTime(hour, minutes);
        _dateTime.SetTotals(totalNumDays, totalNumWeeks);

        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();
        timer = 0f;
        InvokeDateTimeChanged();
    }

    public void Sleep()
    {
        StartNewDayAt(7, 0);
    }

    public void StartNewDayAt(int startHour, int startMinutes)
    {
        _dateTime.StartNewDayAt(startHour, startMinutes);
        CurrentMinutesOfDay = _dateTime.GetMinutesOfDay();
        timer = 0f;
        InvokeDateTimeChanged();
    }

    private void InvokeDateTimeChanged()
    {
        OnDateimeChanged?.Invoke(_dateTime);
        OnDateTimeChanged?.Invoke(_dateTime);
    }
}

[System.Serializable]
public class DateTime
{
    private Days day;
    private int date;
    private int hour;
    private int minutes;

    private int totalNumDays;
    private int totalNumWeeks;

    public Days Day => day;
    public int Date => date;
    public int Hour => hour;
    public int Minutes => minutes;
    public int TotalNumDays => totalNumDays;
    public int TotalNumWeeks => totalNumWeeks;
    public int CurrentWeek => totalNumWeeks % 16 == 0 ? 16 : totalNumWeeks % 16;

    public DateTime(int date, int hour, int minutes)
    {
        this.date = date;
        this.hour = hour;
        this.minutes = minutes;

        totalNumDays = Mathf.Max(1, date);
        totalNumWeeks = 1 + (totalNumDays - 1) / 7;

        day = (Days)(date % 7);
        if (day == 0) day = Days.Sunday;
    }

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