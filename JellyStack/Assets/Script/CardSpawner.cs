using DG.Tweening;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public static CardSpawner Instance { get; private set; }

    [Header("프리팹")]
    [SerializeField] private GameObject cardStackPrefab;
    [Tooltip("CardPackData를 받아 스폰할 PackCard prefab. (BuyPoint와 동일한 prefab을 연결).")]
    [SerializeField] private GameObject packCardPrefab;

    [Header("효과음")]
    [Tooltip("카드(또는 카드팩)가 생성될 때 SoundManager.PlaySFX로 재생할 효과음. 비워두면 무음.")]
    [SerializeField] private AudioClip cardSpawnSfx;

    [Header("점프 애니메이션")]
    [SerializeField] private float jumpPower = 1.5f;
    [SerializeField] private float jumpDuration = 0.55f;

    [Header("카드 배치")]
    [SerializeField] private LayerMask cardMask;
    [SerializeField] private float spawnRadiusMin = 1.5f;
    [SerializeField] private float spawnRadiusMax = 2.5f;
    [SerializeField] private float autoStackRadius = 7f;

    private Vector3 spawnCheckHalfExtents = new Vector3(0.8f, 0.2f, 1.1f);
    private int randomSpawnAttempts = 5;

    private void Awake()
    {
        Instance = this;
    }

    // 소스 위치 주변에 빈자리를 찾아 카드 스폰 (자동 병합 포함)
    public Card SpawnNear(CardData data, Vector3 sourcePos, CardStack excludeStack = null)
    {
        if (data == null || data.cardPrefab == null) return null;

        CardStack sameStack = FindSameCardStackNearby(data, sourcePos, excludeStack);
        if (sameStack != null)
            return SpawnIntoStack(data, sameStack, sourcePos);

        Vector3 landingPos = FindEmptySpawnPosition(sourcePos);
        return Spawn(data, landingPos, sourcePos);
    }

    // 새 카드스택 생성 (worldPos = 정확한 착지점)
    public Card Spawn(CardData data, Vector3 worldPos, Vector3? fromPos = null)
    {
        if (data == null || data.cardPrefab == null)
        {
            Debug.LogError($"Spawn 실패: data 또는 Prefab이 Null");
            return null;
        }

        var stackGo = Instantiate(cardStackPrefab, worldPos, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();

        var cardGo = Instantiate(data.cardPrefab, worldPos, Quaternion.identity);
        var card = cardGo.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"Spawn 실패: Prefab {data.cardPrefab.name}에 Card 컴포넌트가 없습니다.");
            return null;
        }
        card.data = data;
        card.InitializeFromData();

        stack.AddCard(card);

        if (fromPos.HasValue) AnimateJump(card, fromPos.Value, worldPos);

        SoundManager.Instance?.PlaySFX(cardSpawnSfx);

        return card;
    }

    /// <summary>
    /// 카드팩(CardPackData)을 PackCard prefab으로 스폰. BuyPoint.SpawnPack 패턴을 따른다.
    /// 빈 자리를 찾아 PackCard + CardStack을 만들고 packData를 Start() 전에 주입.
    /// 점프 애니메이션은 fromPos(sourcePos)가 주어지면 그 위치에서 튀어나오는 식.
    /// </summary>
    public PackCard SpawnPack(CardPackData packData, Vector3 sourcePos, CardStack excludeStack = null)
    {
        if (packData == null)
        {
            Debug.LogError("SpawnPack 실패: packData가 null");
            return null;
        }
        if (packCardPrefab == null)
        {
            Debug.LogError("SpawnPack 실패: CardSpawner.packCardPrefab이 설정되지 않음 (인스펙터에서 PackCard prefab 할당 필요)");
            return null;
        }

        Vector3 landingPos = FindEmptySpawnPosition(sourcePos);

        var packGo = Instantiate(packCardPrefab, landingPos, Quaternion.identity);
        var packCard = packGo.GetComponent<PackCard>();
        if (packCard == null)
        {
            Debug.LogError($"SpawnPack 실패: {packCardPrefab.name}에 PackCard 컴포넌트가 없습니다.");
            Destroy(packGo);
            return null;
        }

        // Start() 전에 packData 주입 (PackCard.Start가 BuildInitialList를 호출하기 때문)
        packCard.SetPackData(packData);

        // CardPackUI도 있으면 동일하게 데이터 전달 (BuyPoint 패턴과 일치)
        var packUI = packGo.GetComponent<CardPackUI>();
        if (packUI != null) packUI.SetData(packData, null);

        // CardStack으로 감싸기 (없으면 Card.Update가 (0,0,0)으로 끌고 감)
        if (cardStackPrefab != null)
        {
            var stackGo = Instantiate(cardStackPrefab, landingPos, Quaternion.identity);
            var newStack = stackGo.GetComponent<CardStack>();
            if (newStack != null)
                newStack.AddCard(packCard);
            else
                Debug.LogError("SpawnPack: cardStackPrefab에 CardStack 컴포넌트가 없습니다.");
        }

        // 점프 애니메이션 (sourcePos에서 landingPos로)
        AnimateJump(packCard, sourcePos, landingPos);

        SoundManager.Instance?.PlaySFX(cardSpawnSfx);

        return packCard;
    }

    // 기존 스택에 카드 추가
    public Card SpawnIntoStack(CardData data, CardStack targetStack, Vector3? fromPos = null)
    {
        if (data == null || data.cardPrefab == null || targetStack == null)
            return null;

        var cardGo = Instantiate(data.cardPrefab);
        var card = cardGo.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"SpawnIntoStack 실패: Prefab {data.cardPrefab.name}에 Card 컴포넌트가 없습니다.");
            return null;
        }
        card.data = data;
        card.InitializeFromData();

        targetStack.AddCard(card);

        if (fromPos.HasValue) AnimateJump(card, fromPos.Value, targetStack.transform.position);

        SoundManager.Instance?.PlaySFX(cardSpawnSfx);

        return card;
    }

    private void AnimateJump(Card card, Vector3 fromPos, Vector3 landingPos)
    {
        card.transform.position = fromPos;
        card.suppressFollow = true;

        // 점프 시작 위치가 팩과 동일하므로, 이 카드의 콜라이더가 팩 클릭을 가로채지 않도록 비활성화
        var col = card.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        card.transform
            .DOJump(landingPos, jumpPower, 1, jumpDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (card != null)
                {
                    card.suppressFollow = false;
                    // 착지 후 콜라이더 복구 → 이후 정상적으로 클릭/드래그 가능
                    var c = card.GetComponent<Collider>();
                    if (c != null) c.enabled = true;
                }
            });
    }

    private CardStack FindSameCardStackNearby(CardData data, Vector3 center, CardStack exclude)
    {
        if (data == null) return null;

        CardStack[] allStacks = Object.FindObjectsByType<CardStack>(FindObjectsSortMode.None);
        CardStack nearestStack = null;
        float nearestDistance = autoStackRadius;

        foreach (var stack in allStacks)
        {
            if (stack == null || stack == exclude || stack.IsEmpty) continue;
            if (!IsSameCardOnlyStack(stack, data)) continue;

            float distance = GetClosestStackDistance(center, stack);
            if (distance < nearestDistance)
            {
                nearestStack = stack;
                nearestDistance = distance;
            }
        }

        return nearestStack;
    }

    private bool IsSameCardOnlyStack(CardStack stack, CardData data)
    {
        foreach (var card in stack.cards)
        {
            if (card == null || card.data != data) return false;
        }
        return true;
    }

    private float GetClosestStackDistance(Vector3 center, CardStack stack)
    {
        Vector2 centerPos = new Vector2(center.x, center.z);
        float closestDistance = Vector2.Distance(
            centerPos,
            new Vector2(stack.transform.position.x, stack.transform.position.z)
        );

        foreach (var card in stack.cards)
        {
            if (card == null) continue;
            Vector2 cardPos = new Vector2(card.transform.position.x, card.transform.position.z);
            closestDistance = Mathf.Min(closestDistance, Vector2.Distance(centerPos, cardPos));
        }

        return closestDistance;
    }

    private Vector3 FindEmptySpawnPosition(Vector3 center)
    {
        for (int i = 0; i < randomSpawnAttempts; i++)
        {
            Vector3 candidate = GetRandomPositionAround(center);
            if (IsSpawnPositionEmpty(candidate)) return candidate;
        }
        return GetRandomPositionAround(center);
    }

    private bool IsSpawnPositionEmpty(Vector3 position)
    {
        Collider[] hits = Physics.OverlapBox(
            position,
            spawnCheckHalfExtents,
            Quaternion.identity,
            cardMask
        );
        return hits.Length == 0;
    }

    private Vector3 GetRandomPositionAround(Vector3 center)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        return center + new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
    }
}
