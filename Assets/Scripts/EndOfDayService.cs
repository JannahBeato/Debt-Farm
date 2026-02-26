using UnityEngine;
using System;

public class EndOfDayService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerTeleportService teleporter;
    [SerializeField] private CropManager cropManager;
    [SerializeField] private TileManager tileManager;


    [Header("Spawn")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform houseSpawnPoint;

    [Header("Rules")]
    [SerializeField] private SleepRulesSO sleepRules;

    [Header("New Day Start")]
    [SerializeField] private int newDayHour = 7;
    [SerializeField] private int newDayMinutes = 0;

    public event Action EndOfDayStarted;
    public event Action EndOfDayCompleted;

    private bool ending;

    private void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
        if (teleporter == null) teleporter = FindFirstObjectByType<PlayerTeleportService>();
        if (playerEnergy == null && player != null) playerEnergy = player.GetComponent<PlayerEnergy>();
        if (movement == null && player != null) movement = player.GetComponent<PlayerMovement>();
        if (cropManager == null) cropManager = FindFirstObjectByType<CropManager>();
    if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();
    }
    
    public void SleepNow()
    {
        if (ending) return;

        int sleptAt = timeManager != null
            ? timeManager.CurrentDateTime.GetMinutesOfDay()
            : TimeManager.CurrentMinutesOfDay;

        EndDay (EndDayReason.Slept, sleptAt);
    }

    public void PassOutNow()
    {
        if (ending) return;
        EndDay (EndDayReason.PassOut, -1);
    }

    private void EndDay (EndDayReason reason, int sleptAtMinutesOfDay)
    {
        ending = true;
        EndOfDayStarted?.Invoke();

        if (movement != null) movement.SetCanMove(false);

        float wakePercent = sleepRules != null
            ? sleepRules.GetWakePercent(reason, sleptAtMinutesOfDay)
            : 1f;

            // advance day
            if (timeManager != null)
                timeManager.StartNewDayAt(newDayHour, newDayMinutes);
            
            cropManager?.AdvanceDay();
            tileManager?.ClearDailyWatered();

            // apply energy
            if (playerEnergy != null)
                playerEnergy.RestorePercent(wakePercent);

            // teleport
            if (teleporter != null)
                teleporter.Teleport(player, houseSpawnPoint);

            if (movement != null) movement.SetCanMove(true);

            EndOfDayCompleted?.Invoke();
            ending = false;
    }
}