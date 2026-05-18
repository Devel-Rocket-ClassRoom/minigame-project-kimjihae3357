using UnityEngine;

public enum VillagerType
{
    Normal,
    Fire,
    Water,
    Stone
}

[CreateAssetMenu(fileName = "VillagerCardData", menuName = "Card/VillagerCardData")]
public class VillagerCardData : CardData
{
    [Header("주민 카드")]
    public VillagerType villagerType;
    public int maxHealth;
    public int maxHunger;
    public int attackPower;
    public float workSpeed;
}
