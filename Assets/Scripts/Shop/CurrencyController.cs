using System;
using UnityEngine;

public class CurrencyController : MonoBehaviour
{
    public static CurrencyController Instance { get; private set; }

    [SerializeField] private int startingGold = 100;
    [SerializeField] private int playerGold = 100;

    public event Action<int> OnGoldChanged;

    public int CurrentGold => playerGold;
    public int StartingGold => startingGold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerGold = startingGold;
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(playerGold);
    }

    public int GetGold() => playerGold;

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;

        if (playerGold >= amount)
        {
            playerGold -= amount;
            OnGoldChanged?.Invoke(playerGold);
            return true;
        }

        return false;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
    }

    public void SetGold(int amount)
    {
        playerGold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(playerGold);
    }

    public void ResetToStartingGold()
    {
        SetGold(startingGold);
    }
}