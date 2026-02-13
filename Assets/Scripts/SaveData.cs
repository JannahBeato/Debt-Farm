using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public List<InventorySaveData> inventorySaveData;
    public int minutesOfDay;
}
