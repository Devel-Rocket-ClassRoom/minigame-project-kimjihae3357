using System.Collections.Generic;
using UnityEngine;

public class PackCard : Card
{
    [SerializeField] private StarterPack packData;
    [SerializeField] private float spawnRadius = 1.5f;

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

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 targetPos = transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
        CardSpawner.Instance.Spawn(cardData, targetPos, transform.position);

        if (remainingCards.Count == 0)
            Destroy(stack.gameObject);
    }
}
