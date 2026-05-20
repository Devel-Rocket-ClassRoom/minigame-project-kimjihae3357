using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarterPack", menuName = "CardPack/StarterPack")]
public class StarterPack : ScriptableObject
{
    [SerializeField] private List<CardData> cards = new List<CardData>();
}
