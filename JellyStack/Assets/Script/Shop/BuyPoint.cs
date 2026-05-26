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
    [Tooltip("BuyPoint에서 판매되는 카드팩 프리팹 (PackCard 컴포넌트가 붙어 있어야 함)")]
    [SerializeField] private GameObject packPrefab;

    [Tooltip("팩을 감쌀 CardStack 프리팹 (GameManager의 cardStackPrefab과 동일)")]
    [SerializeField] private GameObject cardStackPrefab;

    [Header("결제 조건")]
    [Tooltip("코인 카드 판별용 CardData (CoinData.asset)")]
    [SerializeField] private CardData coinData;

    [Tooltip("팩 1개 구매에 필요한 코인 수")]
    [Min(0)][SerializeField] private int price = 5;

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
        if (stack == null || packPrefab == null) return false;
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
        Vector3 spawnPos = transform.position + spawnOffset;

        var packGo = Instantiate(packPrefab, spawnPos, Quaternion.identity);
        var packCard = packGo.GetComponent<PackCard>();
        if (packCard == null)
        {
            Debug.LogError("BuyPoint: packPrefab에 PackCard 컴포넌트가 없습니다.");
            return;
        }

        // CardStack으로 감싸기 (없으면 그냥 두면 Card.Update가 (0,0,0)으로 끌고 감)
        if (cardStackPrefab != null)
        {
            var stackGo = Instantiate(cardStackPrefab, spawnPos, Quaternion.identity);
            var newStack = stackGo.GetComponent<CardStack>();
            if (newStack != null)
            {
                newStack.AddCard(packCard);
            }
            else
            {
                Debug.LogError("BuyPoint: cardStackPrefab에 CardStack 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("BuyPoint: cardStackPrefab이 비어있어 팩이 CardStack에 감싸지지 않습니다.");
        }
    }
}
