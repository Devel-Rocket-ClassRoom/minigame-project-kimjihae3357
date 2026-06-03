using UnityEngine;

/// <summary>
/// FeedPhase 이후 카드 한도를 체크하는 정산 페이즈 매니저.
/// 플레이어가 보유한 카드 수(EnemyCard / PackCard / CoinCard 제외)가 maxCardAmount를
/// 초과하면, SellPoint로 판매해 한도 이하로 줄여야만 다음 날이 시작된다.
/// 정산 페이즈 동안 게임 시간은 멈춰있지만(DayManager._pendingDayChange=true) 카드
/// 드래그/판매는 자유롭게 가능하다 (InputManager.IsBlocked=false).
/// </summary>
public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    [Header("Card Limit")]
    [Tooltip("플레이어가 가질 수 있는 최대 카드 수 (EnemyCard / PackCard / CoinCard 제외).")]
    [SerializeField] private int maxCardAmount = 20;

    public int MaxCardAmount => maxCardAmount;
    public int CurrentCardCount { get; private set; }
    public bool IsInSettlement { get; private set; }

    private System.Action _onSettlementComplete;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnBeforeSettlement += HandleSettlement;
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnBeforeSettlement -= HandleSettlement;
    }

    private void Update()
    {
        // 평소에도 UI 표시용으로 카운트 추적. 씬의 카드가 수십 장 단위라 매 프레임
        // FindObjectsByType 호출이 부담 없음 — 나중에 수백 장으로 늘어나면 CardSpawner에
        // OnSpawned/OnDespawned 이벤트를 추가해 캐시하는 방식으로 전환 고려.
        CurrentCardCount = CountPlayerCards();

        if (IsInSettlement) TryComplete();
    }

    private void HandleSettlement(System.Action onComplete)
    {
        _onSettlementComplete = onComplete;
        IsInSettlement = true;

        // FeedPhase와 달리 드래그/판매를 허용해야 하므로 IsBlocked는 false 유지.
        // (FeedManager가 종료 시 IsBlocked=false로 풀어둔 상태가 그대로 이어짐)
        InputManager.IsBlocked = false;

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.ShowSettlementOverlay();

        // 이미 한도 이하면 즉시 종료 (정상 경로 — 대부분의 경우).
        // 일단 카운트가 최신인지 보장한 뒤 시도.
        CurrentCardCount = CountPlayerCards();
        TryComplete();
    }

    private void TryComplete()
    {
        if (CurrentCardCount > maxCardAmount) return;

        IsInSettlement = false;

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.HideSettlementOverlay();

        var cb = _onSettlementComplete;
        _onSettlementComplete = null;
        cb?.Invoke();
    }

    private int CountPlayerCards()
    {
        var allCards = Object.FindObjectsByType<Card>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var c in allCards)
        {
            if (c == null) continue;
            if (c is EnemyCard) continue;
            if (c is PackCard) continue;
            // CoinCard는 Card를 상속하지 않고 같은 GameObject에 별도 컴포넌트로 존재한다.
            if (c.GetComponent<CoinCard>() != null) continue;
            count++;
        }
        return count;
    }
}
