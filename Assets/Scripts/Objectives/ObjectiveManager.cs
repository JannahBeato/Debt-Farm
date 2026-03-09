using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Debt Objective")]
    [SerializeField] private int targetGold = 1000;
    [SerializeField] private int deadlineDay = 30; // Player gets all of Day 30. Check when Day 31 starts.

    [Header("Tool Purchase IDs")]
    [SerializeField] private int axeItemID = 4;
    [SerializeField] private int hoeItemID = 5;
    [SerializeField] private int waterBucketItemID = 6;

    [Header("Optional End Screens")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private SaveController saveController;
    private TimeManager timeManager;
    private CurrencyController currencyController;

    private const string OBJ_BUY_AXE = "obj-buy-axe";
    private const string OBJ_BUY_HOE = "obj-buy-hoe";
    private const string OBJ_BUY_WATER_BUCKET = "obj-buy-water-bucket";
    private const string OBJ_PAY_DEBT = "obj-pay-debt";

    private bool gameEnded;
    private bool playerWon;
    private int lastObjectiveDayUpdated = -1;

    public int TargetGold => targetGold;
    public int DeadlineDay => deadlineDay;
    public bool GameEnded => gameEnded;
    public bool PlayerWon => playerWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheRefs();
    }

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        RefreshObjectives();
    }

    private void OnEnable()
    {
        CacheRefs();

        TimeManager.OnDateTimeChanged += HandleDateTimeChanged;

        if (currencyController != null)
            currencyController.OnGoldChanged += HandleGoldChanged;
    }

    private void OnDisable()
    {
        TimeManager.OnDateTimeChanged -= HandleDateTimeChanged;

        if (currencyController != null)
            currencyController.OnGoldChanged -= HandleGoldChanged;
    }

    private void CacheRefs()
    {
        if (saveController == null) saveController = FindObjectOfType<SaveController>();
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        if (currencyController == null) currencyController = FindObjectOfType<CurrencyController>();
    }

    public void LoadState(bool ended, bool won)
    {
        gameEnded = ended;
        playerWon = won;
        RefreshObjectives();
    }

    public void RefreshObjectives()
    {
        CacheRefs();

        EnsureToolPurchaseObjectives();
        EnsureDebtObjective();

        lastObjectiveDayUpdated = timeManager != null ? timeManager.CurrentDateTime.TotalNumDays : -1;

        UpdateDebtObjectiveEntry();
        ApplyEndStateVisuals();

        if (!gameEnded && timeManager != null)
            CheckDebtDeadline(timeManager.CurrentDateTime);
    }

    private void EnsureToolPurchaseObjectives()
    {
        if (JournalManager.Instance == null) return;

        int day = timeManager != null ? timeManager.CurrentDateTime.TotalNumDays : 1;

        if (!JournalManager.Instance.HasEntry(OBJ_BUY_AXE))
        {
            JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
            {
                id = OBJ_BUY_AXE,
                title = "Buy the Axe",
                body = "Visit the shop and buy the Axe. Use it to chop trees in the forest below your farm. Wood can be gathered and sold for extra money.",
                dayAdded = day,
                isObjective = true,
                completed = false
            });
        }

        if (!JournalManager.Instance.HasEntry(OBJ_BUY_HOE))
        {
            JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
            {
                id = OBJ_BUY_HOE,
                title = "Buy the Hoe",
                body = "Visit the shop and buy the Hoe. Use it to till the soil in the fenced area of your farm and prepare land for planting seeds.",
                dayAdded = day,
                isObjective = true,
                completed = false
            });
        }

        if (!JournalManager.Instance.HasEntry(OBJ_BUY_WATER_BUCKET))
        {
            JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
            {
                id = OBJ_BUY_WATER_BUCKET,
                title = "Buy the Water Bucket",
                body = "Visit the shop and buy the Water Bucket. Use it to water your crops every day after planting. Crops need daily watering to continue growing.",
                dayAdded = day,
                isObjective = true,
                completed = false
            });
        }
    }

    private void EnsureDebtObjective()
    {
        if (JournalManager.Instance == null) return;
        if (JournalManager.Instance.HasEntry(OBJ_PAY_DEBT)) return;

        JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
        {
            id = OBJ_PAY_DEBT,
            title = "Pay Back the Debt",
            body = BuildDebtObjectiveBody(),
            dayAdded = 1,
            isObjective = true,
            completed = false
        });
    }

    public void MarkShopPurchaseByItemID(int itemID)
    {
        if (JournalManager.Instance == null) return;

        bool changed = false;

        if (itemID == axeItemID)
            changed = JournalManager.Instance.SetCompleted(OBJ_BUY_AXE, true) || changed;

        if (itemID == hoeItemID)
            changed = JournalManager.Instance.SetCompleted(OBJ_BUY_HOE, true) || changed;

        if (itemID == waterBucketItemID)
            changed = JournalManager.Instance.SetCompleted(OBJ_BUY_WATER_BUCKET, true) || changed;

        if (changed)
            saveController?.SaveGame();
    }

    private void HandleGoldChanged(int newGold)
    {
        if (gameEnded) return;
        UpdateDebtObjectiveEntry();
    }

    private void HandleDateTimeChanged(DateTime dateTime)
    {
        if (gameEnded) return;

        if (dateTime.TotalNumDays != lastObjectiveDayUpdated)
        {
            lastObjectiveDayUpdated = dateTime.TotalNumDays;
            UpdateDebtObjectiveEntry();
        }

        CheckDebtDeadline(dateTime);
    }

    private void CheckDebtDeadline(DateTime dateTime)
    {
        // TotalNumDays starts at 1 on Day 1.
        // Check after Day 30 has fully ended.
        if (dateTime.TotalNumDays <= deadlineDay)
            return;

        bool didWin = currencyController != null && currencyController.CurrentGold >= targetGold;

        if (didWin)
            TriggerWin();
        else
            TriggerLose();
    }

    private void TriggerWin()
    {
        if (gameEnded) return;

        gameEnded = true;
        playerWon = true;

        UpdateDebtObjectiveEntry();
        ApplyEndStateVisuals();
        saveController?.SaveGame();

        Debug.Log("ObjectiveManager: Player won.");
    }

    private void TriggerLose()
    {
        if (gameEnded) return;

        gameEnded = true;
        playerWon = false;

        UpdateDebtObjectiveEntry();
        ApplyEndStateVisuals();
        saveController?.SaveGame();

        Debug.Log("ObjectiveManager: Player lost.");
    }

    private void ApplyEndStateVisuals()
    {
        if (winPanel != null)
            winPanel.SetActive(gameEnded && playerWon);

        if (losePanel != null)
            losePanel.SetActive(gameEnded && !playerWon);

        if (gameEnded)
        {
            Time.timeScale = 0f;
            PauseController.SetPause(true);                
        }
            
    }

    private void UpdateDebtObjectiveEntry()
    {
        if (JournalManager.Instance == null) return;

        JournalManager.Instance.AddOrReplaceEntry(new JournalEntrySaveData
        {
            id = OBJ_PAY_DEBT,
            title = "Pay Back the Debt",
            body = BuildDebtObjectiveBody(),
            dayAdded = 1,
            isObjective = true,
            completed = gameEnded && playerWon
        });
    }

    private string BuildDebtObjectiveBody()
    {
        int gold = currencyController != null ? currencyController.CurrentGold : 0;
        int currentDay = timeManager != null ? timeManager.CurrentDateTime.TotalNumDays : 1;

        if (gameEnded)
        {
            if (playerWon)
                return $"You paid back the debt.\nFinal Gold: {gold} / {targetGold}\nCompleted on Day {currentDay - 1}.";

            return $"You failed to pay back the debt in time.\nFinal Gold: {gold} / {targetGold}\nDeadline: End of Day {deadlineDay}.";
        }

        string status = gold >= targetGold
            ? "Goal currently met. Keep at least 1000 gold until the end of Day 30."
            : "Earn more gold before the deadline.";

        int displayDay = Mathf.Min(currentDay, deadlineDay);

        return
            $"Have at least {targetGold} gold by the end of Day {deadlineDay}.\n" +
            $"Current Gold: {gold} / {targetGold}\n" +
            $"Current Day: {displayDay} / {deadlineDay}\n" +
            $"{status}";
    }
}