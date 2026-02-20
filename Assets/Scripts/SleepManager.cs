using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private GameObject nextDayButton;
    [SerializeField] private FadeUI fadeUI;
    [SerializeField] private Transform player;
    [SerializeField] private Transform houseSpawnPoint;

    [Header("Energy")]
    [SerializeField] private PlayerEnergy playerEnergy;

    private bool _isSleeping;
    private bool _wasPassOut;
    private int _sleepTimeMinutes;

    private void OnEnable()
    {
        TimeManager.OnPassOutTime += HandlePassOut;
    }

    private void OnDisable()
    {
        TimeManager.OnPassOutTime -= HandlePassOut;
    }

    private void SleepNow()
    {
        if (_isSleeping) return;

        _sleepTimeMinutes = TimeManager.CurrentMinutesOfDay;

        Debug.Log("Sleeping...");
        _isSleeping = true;

        Time.timeScale = 0f;
        fadeUI.FadeToBlack();
        nextDayButton.SetActive(true);
    }

    public void GoToNextDay()
    {
        nextDayButton.SetActive(false);
        timeManager.Sleep();
        
        if (playerEnergy != null)
        {
            if (_wasPassOut)
            {
                playerEnergy.RestorePercent(0.2f);
            }
            else if (_sleepTimeMinutes >= 60 && _sleepTimeMinutes < 120)
            {
                playerEnergy.RestorePercent(0.8f);
            }
            else {
                playerEnergy.RestoreToFull();
            }
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.position = houseSpawnPoint.position;
        rb.rotation = houseSpawnPoint.eulerAngles.z;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Time.timeScale = 1f;
        fadeUI.FadeFromBlack();

        _isSleeping = false;
        _wasPassOut = false;

    }

    private void HandlePassOut()
    {
        _wasPassOut = true;
        _sleepTimeMinutes = TimeManager.CurrentMinutesOfDay;
        SleepNow();
    }
}
