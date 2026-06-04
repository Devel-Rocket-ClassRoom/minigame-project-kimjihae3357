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
}
