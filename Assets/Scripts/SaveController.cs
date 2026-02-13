using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using WorldTime;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    // Start is called before the first frame update
    void Start()
    {
        //Define save location
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();

        LoadGame();
    }

    public void SaveGame()
    {
        var worldTime = FindObjectOfType<WorldTime.WorldTime>();

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            minutesOfDay = worldTime != null ? worldTime.GetMinutesOfDay() : 0
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

            var worldTime = FindObjectOfType<WorldTime.WorldTime>();
            if (worldTime != null)
                worldTime.SetMinutesOfDay(saveData.minutesOfDay);
        }
        else
        {
            SaveGame();
        }
    }
}
