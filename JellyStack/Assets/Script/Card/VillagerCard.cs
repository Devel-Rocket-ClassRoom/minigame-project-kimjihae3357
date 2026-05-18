using TMPro;
using UnityEngine;

public class VillagerCard : Card
{

    public int CurrentHealth {get; private set;}
    public int Currenthunger {get; private set;}

    private VillagerCardData VillagerData => data as VillagerCardData;

    private void Awake()
    {
        CurrentHealth = VillagerData.maxHealth;
        Currenthunger = VillagerData.maxHunger;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        NotifyStatChanged();

        if (CurrentHealth == 0)
        {
            //Die();
        }
    }

}
