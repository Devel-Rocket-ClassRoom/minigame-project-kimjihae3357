using UnityEngine;

[CreateAssetMenu(fileName = "SourceCardData", menuName = "Card/SourceCardData")]
public class SourceCardData : CardData
{
    [Header("채집대상")]
    public int GatherCount;
}
