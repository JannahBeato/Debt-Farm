using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    private SaveController saveController;
    private TimeManager timeManager;

    private const string OBJ_PICKUP_AXE = "obj-pickup-axe";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        saveController = FindObjectOfType<SaveController>();
        timeManager = FindObjectOfType<TimeManager>();
    }

    private void OnEnable()
    {
        PlayerItemCollector.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        PlayerItemCollector.OnItemCollected -= HandleItemCollected;
    }

    public void EnsurePickupAxeObjective()
    {
        if (JournalManager.Instance == null) return;
        if (JournalManager.Instance.HasEntry(OBJ_PICKUP_AXE)) return;

        int day = timeManager != null ? timeManager.CurrentDateTime.TotalNumDays : 1;

        JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
        {
            id = OBJ_PICKUP_AXE,
            title = "Pick up the Axe",
            body = "Find the Axe near your home and pick it up.",
            dayAdded = day,
            isObjective = true,
            completed = false
        });

        saveController?.SaveGame();
    }

    private void HandleItemCollected(Item item)
    {
        if (item == null) return;

        bool isAxe =
            (!string.IsNullOrEmpty(item.Name) && item.Name.ToLower().Contains("axe")) ||
            item.gameObject.name.ToLower().Contains("axe");

        if (!isAxe) return;

        if (JournalManager.Instance == null) return;

        if (JournalManager.Instance.SetCompleted(OBJ_PICKUP_AXE, true))
        {
            saveController?.SaveGame();
        }
    }
}