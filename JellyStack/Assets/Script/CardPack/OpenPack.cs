using System.Collections.Generic;
using UnityEngine;

public class PackCard : Card
{
    [SerializeField] private StarterPack packData;

    private List<CardData> remainingCards;

    private void Start()
    {
        remainingCards = packData != null ? packData.GetAllCards() : new List<CardData>();
    }

    public void SpawnNextCard()
    {
        if (remainingCards == null || remainingCards.Count == 0) return;

        CardData cardData = remainingCards[0];
        remainingCards.RemoveAt(0);

        CardSpawner.Instance.SpawnNear(cardData, transform.position);

        if (remainingCards.Count == 0)
            Destroy(stack.gameObject);
    }
}
