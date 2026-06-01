using System.Collections.Generic;
using UnityEngine;

public class PackCard : Card
{
    [SerializeField] private CardPackData packData;   // StarterPack / RandomCardPack 등 어떤 팩 타입이든 받을 수 있음
    [Tooltip("카드 한 장이 나올 때 스폰될 이펙트 프리팹. null이면 이펙트 없음.")]
    [SerializeField] private GameObject spawnEffectPrefab;

    private List<CardData> remainingCards;

    private void Start()
    {
        remainingCards = packData != null ? packData.BuildInitialList() : new List<CardData>();
    }

    public void SpawnNextCard()
    {
        if (remainingCards == null || remainingCards.Count == 0) return;

        CardData cardData = remainingCards[0];
        remainingCards.RemoveAt(0);

        CameraController.Instance?.Shake();
        if (spawnEffectPrefab != null)
            Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
        CardSpawner.Instance.SpawnNear(cardData, transform.position);

        if (remainingCards.Count == 0)
            Destroy(stack.gameObject);
    }
}
