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

    [Tooltip("전투 시 공격 간격(초). 작을수록 자주 때림.")]
    public float attackInterval = 1.0f;

    [Tooltip("true면 아기 카드로 간주되어 채집/제작 레시피에 매칭되지 않음")]
    public bool isBaby = false;

    [Header("아기 성장")]
    [Tooltip("아기(isBaby=true)가 성장했을 때 교체될 성인 VillagerCardData. isBaby=false이면 무시.")]
    public VillagerCardData adultData;
    [Tooltip("태어난 날로부터 몇 일이 지나면 성인으로 변환할지.")]
    public int daysToGrow = 3;
}
