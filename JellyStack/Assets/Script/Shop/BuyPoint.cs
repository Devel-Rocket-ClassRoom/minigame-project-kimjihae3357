using UnityEngine;

/// <summary>
/// 카드 구매 영역. 드래그된 코인 스택이 이 영역에 들어오면
/// 설정된 price만큼 코인을 소모하고 packPrefab을 스폰한다.
/// 스폰된 팩은 CardStack에 감싸져 (0,0,0) 빨려가는 문제 없음.
/// </summary>
public class BuyPoint : MonoBehaviour
{
    // 씬에 여러 BuyPoint가 존재할 수 있으므로 싱글톤을 쓰지 않음.
    // InputManager에서 FindObjectsByType으로 위치별 BuyPoint를 찾음.

    [Header("판매 대상 팩")]
    [Tooltip("[더 이상 사용되지 않음] 스폰은 CardSpawner.packCardPrefab을 사용. 슬롯은 호환을 위해 남겨둠.")]
    [SerializeField] private GameObject packPrefab;

    [Tooltip("스폰될 팩에 주입할 CardPackData. 공유 프리팹 사용 시 여기서 팩 종류를 지정.")]
    [SerializeField] private CardPackData packData;

    [Tooltip("[더 이상 사용되지 않음] CardStack 래핑은 CardSpawner.SpawnPack 내부에서 처리. 슬롯은 호환을 위해 남겨둠.")]
    [SerializeField] private GameObject cardStackPrefab;

    [Header("결제 조건")]
    [Tooltip("코인 카드 판별용 CardData (CoinData.asset)")]
    [SerializeField] private CardData coinData;

    [Tooltip("팩 1개 구매에 필요한 코인 수")]
    [Min(0)][SerializeField] private int price = 3;
    public int Price => price;

    [Header("드롭 영역")]
    [Tooltip("이 콜라이더 위에 카드 스택이 떨어지면 구매 시도. 비워두면 GetComponent로 자동 채워짐.")]
    [SerializeField] private Collider dropArea;

    [Tooltip("BuyPoint 기준점으로부터 팩이 생성될 상대 좌표(월드 축).")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, -2.5f);

    private void Reset()
    {
        // 인스펙터에서 컴포넌트 추가 시 자동으로 자기 자신 콜라이더 연결
        if (dropArea == null) dropArea = GetComponent<Collider>();
    }

    private void Awake()
    {
        // Reset이 호출되지 않은 기존 인스턴스 대비 안전망
        if (dropArea == null) dropArea = GetComponentInChildren<Collider>();
    }

    /// <summary>드롭 위치가 dropArea 콜라이더의 XZ 영역 안에 있는지 (Y는 무시).</summary>
    public bool IsPointInside(Vector3 worldPos)
    {
        if (dropArea == null) return false;
        var b = dropArea.bounds;
        return worldPos.x >= b.min.x && worldPos.x <= b.max.x
            && worldPos.z >= b.min.z && worldPos.z <= b.max.z;
    }

    /// <summary>
    /// 스택의 코인 카드를 price만큼 소비하고 팩 1개 스폰.
    /// 반환: 구매가 실제로 일어났는가 (true)/코인 부족 등으로 무위였는가 (false).
    /// 코인 외 카드는 stack에 남는다 — 호출자가 적절히 드롭 처리.
    /// </summary>
    public bool TryBuy(CardStack stack)
    {
        if (stack == null) return false;
        if (coinData == null) return false;
        if (price <= 0)
        {
            SpawnPack();
            return true;
        }

        // 1) 코인 카드 개수 카운트
        int coinCount = 0;
        for (int i = 0; i < stack.cards.Count; i++)
        {
            var c = stack.cards[i];
            if (c != null && c.data == coinData) coinCount++;
        }

        if (coinCount < price) return false;  // 코인 부족 → 구매 안 함

        // 2) price만큼 코인 소비 (뒤에서부터 안전하게)
        int toRemove = price;
        for (int i = stack.cards.Count - 1; i >= 0 && toRemove > 0; i--)
        {
            var c = stack.cards[i];
            if (c == null || c.data != coinData) continue;

            stack.cards.RemoveAt(i);
            Destroy(c.gameObject);
            toRemove--;
        }
        stack.Refresh();

        // 3) 팩 스폰
        SpawnPack();
        return true;
    }

    private void SpawnPack()
    {
        if (packData == null)
        {
            Debug.LogError("BuyPoint: packData가 설정되지 않음.");
            return;
        }
        if (CardSpawner.Instance == null)
        {
            Debug.LogError("BuyPoint: CardSpawner.Instance가 null — 씬에 CardSpawner가 있는지 확인하세요.");
            return;
        }

        // CardSpawner.SpawnPack에 위임 — 점프 애니메이션 + 효과음 + CardStack 래핑 + PackCard/CardPackUI 데이터 주입까지
        // 일반 카드 스폰과 동일한 연출로 통일된다.
        // sourcePos = BuyPoint 위치 (팩이 BuyPoint에서 튀어나오는 시작점)
        // landing  = BuyPoint + spawnOffset (인스펙터에서 지정한 정확한 착지 위치)
        Vector3 landing = transform.position + spawnOffset;
        var packCard = CardSpawner.Instance.SpawnPack(packData, transform.position, landing);
        if (packCard == null) return;

        // BuyPoint 참조 주입 — CardPackUI가 가격 표시(point.Price)에 사용.
        // (CardSpawner.SpawnPack 내부에서는 BuyPoint를 모르므로 여기서 한 번 더 SetData 호출.)
        var packUI = packCard.GetComponent<CardPackUI>();
        if (packUI != null) packUI.SetData(packData, this);
    }
}
