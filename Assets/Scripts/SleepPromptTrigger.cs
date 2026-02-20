using UnityEngine;

public class SleepPromptTrigger : MonoBehaviour
{
    [SerializeField] private GameObject sleepPromptUI;
    [SerializeField] private SleepManager sleepManager;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            if (sleepPromptUI != null) sleepPromptUI.SetActive(true);
            Time.timeScale = 0f; // pause game while deciding
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            if (sleepPromptUI != null) sleepPromptUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void OnYesPressed()
    {
        if (!playerInside) return;

        if (sleepPromptUI != null) sleepPromptUI.SetActive(false);

        // Do NOT set Time.timeScale here; SleepManager.GoToNextDay will restore it.
        if (sleepManager != null)
            sleepManager.GoToNextDay();
        else
            Time.timeScale = 1f;
    }

    public void OnNoPressed()
    {
        if (sleepPromptUI != null) sleepPromptUI.SetActive(false);
        Time.timeScale = 1f;
    }
}