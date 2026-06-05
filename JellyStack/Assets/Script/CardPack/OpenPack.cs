using System.Collections.Generic;
using UnityEngine;

public class PackCard : Card
{
    [SerializeField] private CardPackData packData;   // StarterPack / RandomCardPack 등 어떤 팩 타입이든 받을 수 있음
    [Header("카드 오픈 이펙트")]
    [SerializeField] private GameObject spawnEffectPrefab;

    private List<CardData> remainingCards;
    private bool _loaded;   // 세이브 복원 시 Start의 BuildInitialList 스킵용

    /// <summary>세이브 저장용 — 연결된 packData 에셋 이름.</summary>
    public string PackDataName => packData != null ? packData.name : "";

    /// <summary>세이브 저장용 — 아직 안 나온 카드 목록.</summary>
    public IReadOnlyList<CardData> GetRemaining()
        => remainingCards ?? new List<CardData>();

    /// <summary>BuyPoint에서 스폰 직후 packData를 동적으로 주입. Start() 전에 호출해야 함.</summary>
    public void SetPackData(CardPackData data)
    {
        packData = data;
    }

    /// <summary>세이브 복원용 — 남은 카드 목록을 직접 주입하고 Start의 재생성을 막는다.</summary>
    public void LoadRemaining(List<CardData> remaining)
    {
        remainingCards = remaining ?? new List<CardData>();
        _loaded = true;
    }

    private void Start()
    {
        if (_loaded) return;   // 복원된 경우 BuildInitialList 건너뜀
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
