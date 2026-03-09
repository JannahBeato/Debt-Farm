using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    private CurrencyController currencyController;
    private ObjectiveManager objectiveManager;
    private CropManager cropManager;

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
        yield return null;
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
        if (currencyController == null) currencyController = FindObjectOfType<CurrencyController>();
        if (objectiveManager == null) objectiveManager = FindObjectOfType<ObjectiveManager>();
        if (cropManager == null) cropManager = FindObjectOfType<CropManager>();

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

            currentGold = currencyController != null ? currencyController.CurrentGold : 0,
            gameEnded = objectiveManager != null && objectiveManager.GameEnded,
            playerWon = objectiveManager != null && objectiveManager.PlayerWon,
            crops = cropManager != null ? cropManager.GetSavedCrops() : null,

            hasSeenIntroLetter = hasSeenIntroLetter,
            journalEntries = journalManager != null ? journalManager.Export() : null,

            shops = GetAllShopSaveData()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        CacheRefs();

        if (!File.Exists(saveLocation))
        {
            hasSeenIntroLetter = false;

            if (journalManager != null)
                journalManager.Import(null);

            if (timeManager != null)
                timeManager.InitializeTime(1, 7, 0);

            if (playerEnergy != null)
                playerEnergy.RestoreToFull();

            if (currencyController != null)
                currencyController.ResetToStartingGold();

            if (objectiveManager != null)
                objectiveManager.LoadState(false, false);

            SaveGame();

            if (introLetterController != null)
                introLetterController.Show();

            return;
        }

        string rawJson = File.ReadAllText(saveLocation);
        SaveData saveData = JsonUtility.FromJson<SaveData>(rawJson);

        bool hasCurrentGoldField = rawJson.Contains("\"currentGold\"");

        if (player != null)
            player.position = saveData.playerPosition;

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

        if (cropManager != null)
            cropManager.LoadSavedCrops(saveData.crops);

        if (playerEnergy != null)
        {
            if (saveData.maxEnergy > 0)
                playerEnergy.LoadEnergy(saveData.currentEnergy, saveData.maxEnergy);
            else
                playerEnergy.RestoreToFull();
        }

        if (currencyController != null)
        {
            int loadedGold = hasCurrentGoldField ? saveData.currentGold : currencyController.StartingGold;
            currencyController.SetGold(loadedGold);
        }

        hasSeenIntroLetter = saveData.hasSeenIntroLetter;

        if (journalManager != null)
            journalManager.Import(saveData.journalEntries);

        if (objectiveManager != null)
            objectiveManager.LoadState(saveData.gameEnded, saveData.playerWon);

        LoadAllShopSaveData(saveData.shops);

        if (!hasSeenIntroLetter && introLetterController != null)
            introLetterController.Show();
    }

    public void MarkIntroLetterSeenAndSave()
    {
        hasSeenIntroLetter = true;
        SaveGame();
    }

    private List<ShopSaveData> GetAllShopSaveData()
    {
        List<ShopSaveData> result = new List<ShopSaveData>();

        ShopNPC[] shops = FindObjectsOfType<ShopNPC>(true);
        foreach (var shop in shops)
        {
            if (shop == null) continue;
            result.Add(shop.GetSaveData());
        }

        return result;
    }

    private void LoadAllShopSaveData(List<ShopSaveData> savedShops)
    {
        if (savedShops == null) return;

        ShopNPC[] sceneShops = FindObjectsOfType<ShopNPC>(true);
        foreach (var sceneShop in sceneShops)
        {
            if (sceneShop == null) continue;

            ShopSaveData matchingSave = savedShops.Find(s => s.shopID == sceneShop.shopID);
            if (matchingSave != null)
                sceneShop.LoadFromSaveData(matchingSave);
        }
    }
}