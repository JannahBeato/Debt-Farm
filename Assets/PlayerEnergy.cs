using System;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private int _maxEnergy = 270;
    [SerializeField] private int _currentEnergy;

    public int MaxEnergy => _maxEnergy;
    public int CurrentEnergy => _currentEnergy;
    public bool IsExhausted => _currentEnergy <= 0;

    public event Action<int, int> OnEnergyChanged;

    private void Awake()
    {
        _currentEnergy = _maxEnergy;
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (_currentEnergy < amount) return false;

        SetEnergy(_currentEnergy - amount);
        return true;
    }

    public void RestorePercent(float percent01)
    {
        percent01 = Mathf.Clamp01(percent01);
        int target = Mathf.RoundToInt(_maxEnergy * percent01);
        SetEnergy(target);
    }

    public void RestoreToFull()
    {
        SetEnergy(_maxEnergy);
    }

    public void Restore(int amount)
    {
        if (amount <= 0) return;
        SetEnergy(_currentEnergy + amount);
    }

    private void SetEnergy(int value)
    {
        _currentEnergy = Mathf.Clamp(value, 0, _maxEnergy);
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }
}
