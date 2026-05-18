using UnityEngine;

[CreateAssetMenu(fileName = "SourceCardData", menuName = "Card/SourceCardData")]
public class SourceCardData : CardData
{
    [Header("채집물 카드")]
    public int GatherCount;
}
