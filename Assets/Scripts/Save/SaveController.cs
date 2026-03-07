using UnityEngine;
using System.Collections;
using System.IO;
using Unity.Cinemachine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private TimeManager timeManager;
    private TileManager tileManager;
    private CinemachineConfiner2D confiner;
    private Transform player;
    private PlayerEnergy playerEnergy;

    // DIARY
    private bool hasSeenIntroLetter;
    private JournalManager journalManager;
    private IntroLetterController introLetterController;

    private void Awake()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        CacheRefs();
    }

    private IEnumerator Start()
    {
        yield return null; // wait 1 frame
        LoadGame();
    }

    private void CacheRefs()
    {
        if (inventoryController == null) inventoryController = FindObjectOfType<InventoryController>();
        if (hotbarController == null) hotbarController = FindObjectOfType<HotbarController>();
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        if (tileManager == null) tileManager = FindObjectOfType<TileManager>();
        if (confiner == null) confiner = FindObjectOfType<CinemachineConfiner2D>();
        if (playerEnergy == null) playerEnergy = FindObjectOfType<PlayerEnergy>();

        if (journalManager == null) journalManager = FindObjectOfType<JournalManager>();
        if (introLetterController == null) introLetterController = FindObjectOfType<IntroLetterController>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public void SaveGame()
    {
        CacheRefs();
        if (player == null || timeManager == null) return;

        SaveData saveData = new SaveData
        {
            playerPosition = player.position,
            mapBoundary = confiner != null && confiner.BoundingShape2D != null ? confiner.BoundingShape2D.name : "",
            inventorySaveData = inventoryController != null ? inventoryController.GetInventoryItems() : null,
            hotbarSaveData = hotbarController != null ? hotbarController.GetHotbarItems() : null,
            modifiedTiles = tileManager != null ? tileManager.GetModifiedTiles() : null,

            date = timeManager.CurrentDateTime.Date,
            hour = timeManager.CurrentDateTime.Hour,
            minutes = timeManager.CurrentDateTime.Minutes,
            totalNumDays = timeManager.CurrentDateTime.TotalNumDays,
            totalNumWeeks = timeManager.CurrentDateTime.TotalNumWeeks,

            currentEnergy = playerEnergy != null ? playerEnergy.CurrentEnergy : 0,
            maxEnergy = playerEnergy != null ? playerEnergy.MaxEnergy : 0,

            // DIARY
            hasSeenIntroLetter = hasSeenIntroLetter,
            journalEntries = journalManager != null ? journalManager.Export() : null
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        CacheRefs();

        // NEW GAME (no save yet)
        if (!File.Exists(saveLocation))
        {
            hasSeenIntroLetter = false;

            if (journalManager != null)
                journalManager.Import(null);

            SaveGame();

            if (introLetterController != null)
                introLetterController.Show();

            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        if (player != null)
            player.position = saveData.playerPosition;

        // Confiner boundary
        if (confiner != null && !string.IsNullOrEmpty(saveData.mapBoundary))
        {
            var boundaryGO = GameObject.Find(saveData.mapBoundary);
            if (boundaryGO != null)
            {
                var poly = boundaryGO.GetComponent<PolygonCollider2D>();
                if (poly != null) confiner.BoundingShape2D = poly;
            }
        }

        if (inventoryController != null && saveData.inventorySaveData != null)
            inventoryController.SetInventoryItems(saveData.inventorySaveData);

        if (hotbarController != null && saveData.hotbarSaveData != null)
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

        if (timeManager != null)
        {
            timeManager.LoadTime(
                saveData.date,
                saveData.hour,
                saveData.minutes,
                saveData.totalNumDays,
                saveData.totalNumWeeks
            );
        }

        if (tileManager != null && saveData.modifiedTiles != null)
            tileManager.LoadModifiedTiles(saveData.modifiedTiles);

        if (playerEnergy != null)
        {
            if (saveData.maxEnergy > 0)
                playerEnergy.LoadEnergy(saveData.currentEnergy, saveData.maxEnergy);
            else
                playerEnergy.RestoreToFull();
        }

        // DIARY LOAD
        hasSeenIntroLetter = saveData.hasSeenIntroLetter;

        if (journalManager != null)
            journalManager.Import(saveData.journalEntries);

        // If not seen yet, show letter
        if (!hasSeenIntroLetter && introLetterController != null)
            introLetterController.Show();
    }

    public void MarkIntroLetterSeenAndSave()
    {
        hasSeenIntroLetter = true;
        SaveGame();
    }
}