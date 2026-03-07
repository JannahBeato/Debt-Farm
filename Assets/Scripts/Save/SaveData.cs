using UnityEngine;
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

    // GOLD
    public int currentGold;

    // GAME RESULT
    public bool gameEnded;
    public bool playerWon;

    // DIARY / TUTORIAL
    public bool hasSeenIntroLetter;
    public List<JournalEntrySaveData> journalEntries;
}

[System.Serializable]
public class JournalEntrySaveData
{
    public string id;
    public string title;
    public string body;
    public int dayAdded;

    public bool isObjective;
    public bool completed;
}