using TMPro;
using UnityEngine;

public class VillagerCard : Card
{

    public int CurrentHealth {get; private set;}
    public int Currenthunger {get; private set;}

    private VillagerCardData VillagerData => data as VillagerCardData;
    public int MaxHealth => VillagerData != null ? VillagerData.maxHealth : 0;

    private void Awake()
    {
        InitializeFromData();
    }

    public override void InitializeFromData()
    {
        if (VillagerData == null)
            return;

        CurrentHealth = VillagerData.maxHealth;
        Currenthunger = VillagerData.maxHunger;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        NotifyStatChanged();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Feed(int amount)
    {
        Currenthunger = Mathf.Max(0, Currenthunger - amount);
        NotifyStatChanged();
    }

    public void Heal(int amount)
    {
        if (VillagerData == null || amount <= 0) return;
        CurrentHealth = Mathf.Min(VillagerData.maxHealth, CurrentHealth + amount);
        NotifyStatChanged();
    }

    public void ResetHunger()
    {
        if (VillagerData == null) return;
        Currenthunger = VillagerData.maxHunger;
        NotifyStatChanged();
    }

}
