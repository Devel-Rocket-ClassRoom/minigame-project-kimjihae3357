using System.Collections.Generic;
using UnityEngine;

public class PackCard : Card
{
    [SerializeField] private CardPackData packData;   // StarterPack / RandomCardPack 등 어떤 팩 타입이든 받을 수 있음
    [Header("카드 오픈 이펙트")]
    [SerializeField] private GameObject spawnEffectPrefab;

    private List<CardData> remainingCards;

    /// <summary>BuyPoint에서 스폰 직후 packData를 동적으로 주입. Start() 전에 호출해야 함.</summary>
    public void SetPackData(CardPackData data)
    {
        packData = data;
    }

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
