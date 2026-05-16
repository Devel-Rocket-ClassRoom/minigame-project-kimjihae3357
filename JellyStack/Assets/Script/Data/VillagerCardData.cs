using UnityEngine;

[CreateAssetMenu(fileName = "VillagerCardData", menuName = "Card/VillagerCardData")]
public class VillagerCardData : CardData
{
    [Header ("주민 카드")]
    public int maxHealth;
    public int maxHunger;
    public int attackPower;
    public float workSpeed;
}
