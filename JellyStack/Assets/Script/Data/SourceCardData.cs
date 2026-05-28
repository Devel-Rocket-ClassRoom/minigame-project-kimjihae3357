using UnityEngine;

[CreateAssetMenu(fileName = "SourceCardData", menuName = "Card/SourceCardData")]
public class SourceCardData : CardData
{
    [Header("채집물 카드")]
    public int GatherCount;

    [Header("채집 설정")]
    [Tooltip("채집 결과물 카드")]
    public CardData gatherResult;
    [Tooltip("채집 결과물 기본 수량")]
    public int gatherResultCount = 1;
    [Tooltip("채집 소요 시간(초). 채집물마다 다르게 설정 가능.")]
    public float gatherDuration = 3f;
}
