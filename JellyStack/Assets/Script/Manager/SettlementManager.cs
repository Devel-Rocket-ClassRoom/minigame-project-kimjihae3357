using UnityEngine;

/// <summary>
/// FeedPhase 이후 카드 한도를 체크하는 정산 페이즈 매니저.
/// 플레이어가 보유한 카드 수(EnemyCard / PackCard / CoinCard 제외)가 maxCardAmount를
/// 초과하면, SellPoint로 판매해 한도 이하로 줄여야만 다음 날이 시작된다.
/// 정산 페이즈 동안 작업 진행은 멈추지만 카드 드래그/판매는 자유롭게 가능하다.
/// </summary>
public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    [Header("Card Limit")]
    [Tooltip("플레이어가 가질 수 있는 기본 최대 카드 수 (EnemyCard / PackCard / CoinCard 제외).")]
    [SerializeField] private int maxCardAmount = 20;

    [Header("카운트 제외 카드")]
    [Tooltip("카드 수 계산에서 제외할 CoinData 에셋.")]
    [SerializeField] private CardData coinCardData;

    [Header("Storage 카드 보너스")]
    [Tooltip("Storage 카드 판별용 CardData. 인스펙터에서 StorageCardData 에셋 연결.")]
    [SerializeField] private CardData storageCardData;
    [Tooltip("Storage 카드 1장당 증가하는 최대 카드 수.")]
    [SerializeField] private int storageBonus = 5;

    /// <summary>기본 한도 + Storage 카드 보너스를 합산한 실제 최대 카드 수.</summary>
    public int MaxCardAmount => maxCardAmount + CountStorageCards() * storageBonus;
    public int CurrentCardCount { get; private set; }
    public bool IsInSettlement { get; private set; }

    private System.Action _onSettlementComplete;
    private bool pausedProgressTasks;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        ResumeProgressTasks();
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

        ResumeProgressTasks();
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

        InputManager.IsBlocked = false;
        PauseProgressTasks();

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.ShowSettlementOverlay();
        else
            Debug.LogWarning("[Settlement] UI_Ingame.Instance가 null — 오버레이 표시 불가");

        CurrentCardCount = CountPlayerCards();
        Debug.Log($"[Settlement] 시작 — 현재:{CurrentCardCount} / 한도:{MaxCardAmount}");
        TryComplete();
    }

    /// <summary>
    /// 카드 판매 후 한 프레임 뒤에 갱신 요청.
    /// Destroy()는 프레임 끝에 적용되므로 같은 프레임에 카운팅하면 아직 카드가 살아있음.
    /// yield return null은 timeScale=0에서도 다음 프레임을 기다림.
    /// </summary>
    public void RequestRefresh()
    {
        StartCoroutine(RefreshNextFrame());
    }

    private System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null; // Destroy()가 실제 적용되는 다음 프레임까지 대기
        RefreshCount();
    }

    public void RefreshCount()
    {
        CurrentCardCount = CountPlayerCards();
        if (IsInSettlement) TryComplete();
    }

    private void TryComplete()
    {
        if (CurrentCardCount > MaxCardAmount) return;

        IsInSettlement = false;
        ResumeProgressTasks();

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.HideSettlementOverlay();

        var cb = _onSettlementComplete;
        _onSettlementComplete = null;
        cb?.Invoke();
    }

    private void PauseProgressTasks()
    {
        ProgressTask.IsPaused = true;
        pausedProgressTasks = true;
    }

    private void ResumeProgressTasks()
    {
        if (!pausedProgressTasks) return;

        ProgressTask.IsPaused = false;
        pausedProgressTasks = false;
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
            if (c.data == coinCardData) continue;
            count++;
        }
        return count;
    }

    private int CountStorageCards()
    {
        if (storageCardData == null) return 0;
        var allCards = Object.FindObjectsByType<Card>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var c in allCards)
        {
            if (c != null && c.data == storageCardData) count++;
        }
        return count;
    }
}
