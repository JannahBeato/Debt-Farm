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
    public List<TileStateSaveData> modifiedTiles;

    // TIME
    public int date;
    public int hour;
    public int minutes;
    public int totalNumDays;
    public int totalNumWeeks;

    // ENERGY
    public int currentEnergy;
    public int maxEnergy;

    // DIARY / TUTORIAL
    public bool hasSeenIntroLetter;
    public List<JournalEntrySaveData> journalEntries;
}

[System.Serializable]
public class JournalEntrySaveData
{
    public string id;        // "intro-letter", "day1", etc.
    public string title;     // "A Letter From..."
    public string body;      // full text
    public int dayAdded;     // optional (use your time system)

}