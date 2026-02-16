using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;

    // NEW TIME DATA
    public int date;
    public int hour;
    public int minutes;
    public int totalNumDays;
    public int totalNumWeeks;
}
