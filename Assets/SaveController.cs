using UnityEngine;
using System.IO;
using Unity.Cinemachine;
using WorldTime;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    public void SaveGame()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var confiner = FindObjectOfType<CinemachineConfiner2D>();
        var worldTime = FindObjectOfType<WorldTime.WorldTime>();

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = confiner.BoundingShape2D.name,
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
