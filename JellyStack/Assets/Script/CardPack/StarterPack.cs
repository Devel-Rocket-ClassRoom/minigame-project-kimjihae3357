using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct CardEntry
{
    public CardData data;
    public int count;
}

[CreateAssetMenu(fileName = "StarterPack", menuName = "CardPack/StarterPack")]
public class StarterPack : CardPackData
{
    [SerializeField] private List<CardEntry> cardEntries = new List<CardEntry>();
   
    public List<CardData> GetAllCards()
    {
        var result = new List<CardData>();
        foreach (var entry in cardEntries)
        {
            for (int i = 0; i < entry.count; i++)
                result.Add(entry.data);
        }
        return result;
    }
}
