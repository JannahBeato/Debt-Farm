using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [SerializeField] private EndOfDayService endOfDay;

    private void Awake()
    {
        if (endOfDay == null) endOfDay = FindFirstObjectByType<EndOfDayService>();
    }

    public void GoToNextDay()
    {
        if (endOfDay == null)
        {
            Debug.LogError("SleepManager: EndOfDayService missing.");
            return;
        }

        endOfDay.SleepNow();
    }
}