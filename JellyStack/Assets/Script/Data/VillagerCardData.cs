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

    [Tooltip("true면 아기 카드로 간주되어 채집/제작 레시피에 매칭되지 않음")]
    public bool isBaby = false;
}
