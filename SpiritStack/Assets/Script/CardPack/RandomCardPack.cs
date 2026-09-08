using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WeightedCardEntry
{
    public CardData data;
    [Min(0)] public int weight;   // 가중치 (0이면 절대 안 뽑힘)
}

[CreateAssetMenu(fileName = "RandomCardPack", menuName = "CardPack/RandomCardPack")]
public class RandomCardPack : CardPackData
{
    [Tooltip("각 카드의 가중치. 전체 합 대비 비율로 추첨됨 (with replacement)")]
    [SerializeField] private List<WeightedCardEntry> cardEntries = new List<WeightedCardEntry>();

    [Tooltip("팩 1개당 총 몇 장을 뽑을지")]
    [Min(0)][SerializeField] private int drawCount = 5;

    public override List<CardData> BuildInitialList()
    {
        var result = new List<CardData>();
        if (drawCount <= 0 || cardEntries == null || cardEntries.Count == 0) return result;

        int totalWeight = 0;
        foreach (var e in cardEntries)
            if (e.data != null) totalWeight += Mathf.Max(0, e.weight);
        if (totalWeight <= 0) return result;

        for (int i = 0; i < drawCount; i++)
        {
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var e in cardEntries)
            {
                if (e.data == null) continue;
                cumulative += Mathf.Max(0, e.weight);
                if (roll < cumulative)
                {
                    result.Add(e.data);
                    break;
                }
            }
        }
        return result;
    }
}
