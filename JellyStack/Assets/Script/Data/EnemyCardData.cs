using UnityEngine;


[CreateAssetMenu(fileName = "EnemyCardData", menuName = "Card/EnemyCardData")]
public class EnemyCardData : CardData
{
    [Header("적 카드")]
    public int maxHealth;
    public int attackPower;

    [Tooltip("점프 간격(초). 작을수록 자주 점프해 빠르게 추적함.")]
    public float jumpInterval = 2.0f;

    [Tooltip("전투 시 공격 간격(초). 작을수록 자주 때림.")]
    public float attackInterval = 1.0f;
}
