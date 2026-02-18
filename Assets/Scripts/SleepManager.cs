using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private GameObject nextDayButton;
    [SerializeField] private FadeUI fadeUI;
    [SerializeField] private Transform player;
    [SerializeField] private Transform houseSpawnPoint;

    private void OnEnable()
    {
        TimeManager.OnSleepTimeReached += HandleSleep;
    }

    private void OnDisable()
    {
        TimeManager.OnSleepTimeReached -= HandleSleep;
    }

    private void HandleSleep()
    {
        Debug.Log("2AM reached. Sleeping...");

        Time.timeScale = 0f;

        fadeUI.FadeToBlack();

        nextDayButton.SetActive(true);
    }

    public void GoToNextDay()
    {
        nextDayButton.SetActive(false);
        timeManager.Sleep();
        Time.timeScale = 1f;
        fadeUI.FadeFromBlack();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        rb.position = houseSpawnPoint.position;
        rb.rotation = houseSpawnPoint.eulerAngles.z;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

    }
}
