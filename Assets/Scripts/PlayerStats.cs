using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Level & XP")]
    public int level = 1;
    public int currentXP;
    public int xpToNextLevel = 10;
    public int killCount;

    [Header("Upgradeable Stats")]
    public int damage = 20;
    public float attackInterval = 0.8f;
    public float gemPickupRadius = 3f;
    public int defense = 0;
    public float xpMultiplier = 1f;
    public int gold = 0;

    public event Action OnLevelUp;
    public event Action OnStatsChanged;

    void Awake() => Instance = this;

    public void AddXP(int amount)
    {
        currentXP += Mathf.RoundToInt(amount * xpMultiplier);
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.2f);
            level++;
            OnLevelUp?.Invoke();
        }
        OnStatsChanged?.Invoke();
    }

    public void AddKill() => killCount++;
}
