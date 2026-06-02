using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SourceCardData", menuName = "Card/SourceCardData")]
public class SourceCardData : CardData
{
    [Header("채집물 카드")]
    public int GatherCount;

    [Header("채집 설정")]
    [Tooltip("채집 결과물 카드 (Gather Results가 비어있을 때 사용)")]
    public CardData gatherResult;
    [Tooltip("채집 결과물 기본 수량")]
    public int gatherResultCount = 1;
    [Tooltip("채집 소요 시간(초). 채집물마다 다르게 설정 가능.")]
    public float gatherDuration = 3f;

    [Header("채집 결과 (랜덤)")]
    [Tooltip("가중치 기반 랜덤 채집 결과 목록. 비어있으면 위의 Gather Result를 사용.")]
    public List<WeightedCardEntry> gatherResults;
}
