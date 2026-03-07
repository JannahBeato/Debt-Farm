using UnityEngine;

public class SleepPromptTrigger : MonoBehaviour
{
    [SerializeField] private GameObject sleepPromptUI;
    [SerializeField] private SleepUIController sleepUI;   // NEW
    [SerializeField] private PlayerMovement playerMovement;

    private bool playerInside;

    private void Awake()
    {
        if (sleepUI == null) sleepUI = FindFirstObjectByType<SleepUIController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = true;
        if (playerMovement == null) playerMovement = collision.GetComponent<PlayerMovement>();

        if (sleepPromptUI != null) sleepPromptUI.SetActive(true);

        // Don’t pause world; just stop player
        if (playerMovement != null) playerMovement.SetCanMove(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = false;
        if (sleepPromptUI != null) sleepPromptUI.SetActive(false);

        if (playerMovement != null) playerMovement.SetCanMove(true);
    }

    public void OnYesPressed()
    {
        if (!playerInside) return;

        if (sleepPromptUI != null) sleepPromptUI.SetActive(false);

        // allow UI interaction while player stays “inactive”
        if (playerMovement != null) playerMovement.SetCanMove(false);

        // ✅ show fade + next day button
        sleepUI?.ShowSleepUI();
    }

    public void OnNoPressed()
    {
        if (sleepPromptUI != null) sleepPromptUI.SetActive(false);
        if (playerMovement != null) playerMovement.SetCanMove(true);
    }
}