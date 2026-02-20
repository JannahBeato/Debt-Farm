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

    // If you do NOT use PlayerPassOut, SleepManager can handle pass-out UI at 2:00 AM.
    // If PlayerPassOut exists in the scene, SleepManager will NOT subscribe (prevents double-ending the day).
    private bool _subscribedToPassOut;

    private void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
        if (playerEnergy == null && player != null) playerEnergy = player.GetComponent<PlayerEnergy>();
    }

    private void OnEnable()
    {
        // IMPORTANT FIX:
        // Do NOT sleep at midnight. That prevented reaching 01:00 entirely.
        // We only optionally handle pass-out at 02:00.
        if (FindFirstObjectByType<PlayerPassOut>() == null)
        {
            TimeManager.OnPassOutTime += HandlePassOutTime;
            _subscribedToPassOut = true;
        }
    }

    private void OnDisable()
    {
        if (_subscribedToPassOut)
        {
            TimeManager.OnPassOutTime -= HandlePassOutTime;
            _subscribedToPassOut = false;
        }
    }

    private void HandlePassOutTime()
    {
        Debug.Log("2:00 AM reached. Passing out...");

        Time.timeScale = 0f;

        if (fadeUI != null) fadeUI.FadeToBlack();
        if (nextDayButton != null) nextDayButton.SetActive(true);
    }

    // This is called by your bed prompt (and by the nextDayButton if you use it).
    public void GoToNextDay()
    {
        if (timeManager == null)
        {
            Debug.LogError("SleepManager: TimeManager reference missing.");
            return;
        }

        // Capture sleep time BEFORE changing time to the next day.
        int sleptHour = timeManager.CurrentDateTime.Hour;
        int sleptMinute = timeManager.CurrentDateTime.Minutes;

        float wakePercent = GetWakeEnergyPercent(sleptHour);

        if (nextDayButton != null) nextDayButton.SetActive(false);

        // Advance to next day 07:00
        timeManager.Sleep();

        // Restore energy for the new day
        if (playerEnergy == null && player != null) playerEnergy = player.GetComponent<PlayerEnergy>();
        if (playerEnergy != null)
        {
            playerEnergy.RestorePercent(wakePercent);
        }

        // Resume and fade back
        Time.timeScale = 1f;
        if (fadeUI != null) fadeUI.FadeFromBlack();

        // Teleport player home
        if (player != null && houseSpawnPoint != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = houseSpawnPoint.position;
                rb.rotation = houseSpawnPoint.eulerAngles.z;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else
            {
                // Fallback if no Rigidbody2D
                player.position = houseSpawnPoint.position;
                player.rotation = houseSpawnPoint.rotation;
            }
        }

        Debug.Log("Slept at " + sleptHour.ToString("00") + ":" + sleptMinute.ToString("00") +
                  " -> Wake energy " + (wakePercent * 100f).ToString("0") + "%");
    }

    // Rules:
    // - Before 01:00 -> 100%  (this includes 07:00-23:59 and 00:00-00:59)
    // - 01:00-01:59 -> 80%
    // - 02:00-06:59 -> 20%
    private float GetWakeEnergyPercent(int sleptHour)
    {
        if (sleptHour >= 7) return 1.0f; // evening/night of same day
        if (sleptHour == 0) return 1.0f; // 00:xx
        if (sleptHour == 1) return 0.8f; // 01:xx
        return 0.2f;                     // 02:xx-06:xx
    }
}