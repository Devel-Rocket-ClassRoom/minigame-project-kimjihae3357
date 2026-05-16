using TMPro;
using UnityEngine;

public class VillagerCard : Card
{

    public int currentHealth {get; private set;}
    public int currenthunger {get; private set;}

    public System.Action OnStatChanged;

    private VillagerCardData VillagerData => data as VillagerCardData;

    private void Awake()
    {
        currentHealth = VillagerData.maxHealth;
        currenthunger = VillagerData.maxHunger;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth == 0)
        {
            //Die();
        }
    }

}
