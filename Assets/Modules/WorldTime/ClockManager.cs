using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI Date, Time, Season, Week;

    private void OnEnable()
    {
        TimeManager.OnDateTimeChanged += UpdateDateTime;
    }

    private void OnDisable()
    {
        TimeManager.OnDateTimeChanged -= UpdateDateTime;
    }

    private void UpdateDateTime(DateTime dateTime)
    {
        Date.text = $"Day: {dateTime.Date}";
        Time.text = $"Time: {dateTime.Hour:00}:{dateTime.Minutes:00}";
    }

}
