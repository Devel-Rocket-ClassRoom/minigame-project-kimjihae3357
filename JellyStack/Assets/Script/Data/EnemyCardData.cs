using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DropEntry
{
    public CardData card;
    [Range(0f, 1f), Tooltip("드롭 확률 (0 = 0%, 1 = 100%)")]
    public float chance;
}

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

    [Header("드롭 테이블")]
    [Tooltip("사망 시 각 항목을 독립적으로 확률 판정. 여러 카드가 동시에 드롭될 수 있음.")]
    public List<DropEntry> dropTable;
}
