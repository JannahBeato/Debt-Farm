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
            sleepPromptUI.SetActive(true);
            Time.timeScale = 0f; // pause game
        }
    }

    public void OnYesPressed() 
    {
        sleepPromptUI.SetActive(false);
        Time.timeScale = 1f;
        sleepManager.GoToNextDay();
    }

    public void OnNoPressed() 
    {
        sleepPromptUI.SetActive(false);
        Time.timeScale = 1f;
    }
}
