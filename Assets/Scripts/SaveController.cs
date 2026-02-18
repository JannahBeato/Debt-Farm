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

    // Start is called before the first frame update
    void Start()
    {
        //Define save location
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        timeManager = FindObjectOfType<TimeManager>();

        LoadGame();
    }

    public void SaveGame()
    {
        var TimeManager = FindObjectOfType<TimeManager>();

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            
            // SAVE TIME DATA
            date = TimeManager.CurrentDateTime.Date,
            hour = TimeManager.CurrentDateTime.Hour,
            minutes = TimeManager.CurrentDateTime.Minutes,
            totalNumDays = TimeManager.CurrentDateTime.TotalNumDays,
            totalNumWeeks = TimeManager.CurrentDateTime.TotalNumWeeks
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

            FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D =
                GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

            timeManager.InitializeTime( 
                saveData.date,
                saveData.hour,
                saveData.minutes
            );  

            timeManager.CurrentDateTime.SetTotals(saveData.totalNumDays, saveData.totalNumWeeks);
            
        }
        else
        {
            SaveGame();
            return;
        }
    }
}
