using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SourceCardData", menuName = "Card/SourceCardData")]
public class SourceCardData : CardData
{
    [Header("Gathering Source")]
    public int GatherCount;

    [Header("Gather Settings")]
    [Tooltip("Base number of result cards created per gather.")]
    public int gatherResultCount = 1;

    [Tooltip("Gather duration in seconds.")]
    public float gatherDuration = 3f;

    [Header("Random Gather Results")]
    [Tooltip("Weighted random gather result list.")]
    public List<WeightedCardEntry> gatherResults = new List<WeightedCardEntry>();

    [Header("Gather Effect")]
    [Tooltip("채집 진행 중 카드 위에 표시할 이펙트 (선택). 비워두면 이펙트 없음. 예: 돌 채집 시 돌가루 이펙트.")]
    public GameObject gatherEffectPrefab;
}
