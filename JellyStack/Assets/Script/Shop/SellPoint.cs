using UnityEngine;

/// <summary>
/// 카드 판매 영역. 드래그된 스택이 일정 거리 안에 떨어지면
/// 판매 가능한 카드를 판매하고 sellPrice 합만큼 CoinCard를 스폰한다.
/// </summary>
public class SellPoint : MonoBehaviour
{
    // 씬에 여러 SellPoint가 존재할 수 있으므로 싱글톤을 쓰지 않음.
    // InputManager에서 FindObjectsByType으로 위치별 SellPoint를 찾음.

    [Header("판매 데이터")]
    [SerializeField] private CardData coinData;   // CoinData.asset 할당

    [Header("드롭 영역")]
    [Tooltip("이 콜라이더 위에 카드 스택이 떨어지면 판매 시도. 비워두면 GetComponent로 자동 채워짐.")]
    [SerializeField] private Collider dropArea;

    [Header("코인 스폰")]
    [Tooltip("SellPoint 기준점으로부터 코인이 생성될 상대 좌표(월드 축 기준). 기본: 살짝 아래쪽(-Z).")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, -2.5f);

    private void Reset()
    {
        if (dropArea == null) dropArea = GetComponent<Collider>();
    }

    private void Awake()
    {
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
