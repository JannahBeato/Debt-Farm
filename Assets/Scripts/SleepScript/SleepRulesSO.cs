using UnityEngine;

public enum EndDayReason { Slept, PassOut }

[CreateAssetMenu(menuName="Game/Sleep Rules")]
public class SleepRulesSO : ScriptableObject
{
    [Header("Wake Energy Percents")]
    [Range(0f, 1f)] public float before1AM = 1.0f; 
    [Range(0f, 1f)] public float between1And2 = 0.8f;
    [Range(0f, 1f)] public float after2OrPassOut = 0.2f;

    public float GetWakePercent (EndDayReason reason, int sleptAtMinutesOfDay)
    {
        if (reason == EndDayReason.PassOut || sleptAtMinutesOfDay < 0)
            return after2OrPassOut;
        
        int h = sleptAtMinutesOfDay / 60;

        if (h >= 7) return before1AM;
        if (h == 0) return before1AM;
        if (h == 1) return between1And2;
        return after2OrPassOut;
    }
}
