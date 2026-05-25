using UnityEngine;

/// <summary>
/// 카드 판매 영역. 드래그된 스택이 일정 거리 안에 떨어지면
/// 판매 가능한 카드를 판매하고 sellPrice 합만큼 CoinCard를 스폰한다.
/// </summary>
public class SellPoint : MonoBehaviour
{
    public static SellPoint Instance { get; private set; }

    [Header("판매 데이터")]
    [SerializeField] private CardData coinData;   // CoinData.asset 할당
    [SerializeField] private float dropRadius = 2.0f;

    [Header("코인 스폰")]
    [Tooltip("SellPoint 기준점으로부터 코인이 생성될 상대 좌표(월드 축 기준). 기본: 살짝 아래쪽(-Z).")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, -2.5f);

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>드래그 스택의 위치가 SellPoint 범위 안에 있는지 (XZ 거리)</summary>
    public bool IsInRange(Vector3 worldPos)
    {
        Vector3 a = new Vector3(worldPos.x, 0f, worldPos.z);
        Vector3 b = new Vector3(transform.position.x, 0f, transform.position.z);
        return Vector3.Distance(a, b) <= dropRadius;
    }

    /// <summary>
    /// 스택 내 CanSell=true 카드만 판매 처리. 판매 불가 카드는 stack에 남는다.
    /// 반환: 스택이 완전히 비어 호출자가 stack.gameObject를 파괴해도 되는지 여부.
    /// </summary>
    public bool SellStack(CardStack stack)
    {
        if (stack == null) return false;

        int totalPrice = 0;

        // 뒤에서 앞으로 순회하며 판매 가능한 카드 제거 (인덱스 안전)
        for (int i = stack.cards.Count - 1; i >= 0; i--)
        {
            var c = stack.cards[i];
            if (c == null || c.data == null) continue;
            if (!c.data.CanSell) continue;

            totalPrice += Mathf.Max(0, c.data.sellPrice);
            stack.cards.RemoveAt(i);
            Destroy(c.gameObject);
        }

        // 판매가격만큼 CoinCard 스폰 — SellPoint 본체가 아닌 spawnOffset 만큼 떨어진 지점을 기준으로
        // CardSpawner.SpawnNear는 같은 종류 스택이 근처에 있으면 알아서 그 스택에 쌓아준다.
        // 첫 코인이 spawnPos 주변(반경 1.5~2.5)에 떨어지고, 이후 코인은 그 스택에 자동 합류.
        if (totalPrice > 0 && coinData != null && CardSpawner.Instance != null)
        {
            Vector3 spawnPos = transform.position + spawnOffset;
            for (int i = 0; i < totalPrice; i++)
                CardSpawner.Instance.SpawnNear(coinData, spawnPos);
        }

        return stack.IsEmpty;
    }
}
