using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 7일마다 등장하는 날씨 룰렛 UI 컨트롤러.
/// 룰렛 표시 → 무작위 날씨로 회전 후 정지 → 결과 머무름 → 자동 숨김.
/// 결과 날씨 적용 로직은 후속 작업.
/// </summary>
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("룰렛 UI")]
    [Tooltip("룰렛 전체를 감싸는 CanvasGroup (보이기/숨기기 제어).")]
    [SerializeField] private CanvasGroup rouletteGroup;

    [Tooltip("4분할 룰렛 휠 RectTransform. 휠 이미지 배치: 12시=Sunny, 3시=Rain, 6시=Snow, 9시=Storm 이어야 함.")]
    [SerializeField] private RectTransform wheel;

    [Header("룰렛 회전 설정")]
    [SerializeField] private float spinDuration = 3.0f;
    [Tooltip("스핀 중 추가 전체 회전 수 (보여지는 회전 횟수).")]
    [SerializeField] private int extraSpins = 5;
    [Tooltip("결과 정착 후 룰렛이 화면에 머무는 시간(초).")]
    [SerializeField] private float showResultDuration = 1.2f;

    [Header("날씨 적용 기간")]
    [SerializeField] private int weatherDurationDays = 3;

    [Header("햇살 효과")]
    [SerializeField] private GameObject sunnyEffect;
    [Tooltip("맑음(Sunny) 날씨일 때 자원 채집 결과가 2배가 될 확률 (0~1).")]
    [SerializeField, Range(0f, 1f)] private float sunnyDoubleChance = 0.3f;

    [Header("날씨 효과")]
    [Tooltip("씬에 배치된 비 파티클(메인 카메라 자식, 초기 비활성). Weather_Rain.prefab 인스턴스.")]
    [SerializeField] private GameObject rainEffect;
    [Tooltip("비 올 때 자원 채집 속도 배율 (1보다 크면 빨라짐).")]
    [SerializeField] private float rainSpeedMultiplier = 1.5f;

    [Header("눈 효과")]
    [Tooltip("씬에 배치된 눈 파티클(메인 카메라 자식, 초기 비활성). Weather_Snow.prefab 인스턴스.")]
    [SerializeField] private GameObject snowEffect;
    [Tooltip("얼어붙은 카드 위에 덮을 얼음 비주얼 프리팹 (ice).")]
    [SerializeField] private GameObject icePrefab;
    [Tooltip("분리된 얼음 카드를 담을 CardStack 프리팹 (GameManager의 것과 동일).")]
    [SerializeField] private GameObject cardStackPrefab;
    [Tooltip("하루에 얼리는 카드 수.")]
    [SerializeField] private int freezeCount = 2;

    [Header("태풍 효과")]
    [SerializeField] private GameObject stormEffect;
    [Tooltip("카드가 날아갈 때 카드에 붙어 따라가는 이펙트 프리팹 (카드 파괴 시 함께 제거됨).")]
    [SerializeField] private GameObject flyAwayEffectPrefab;
    [Tooltip("하루에 흔들리는 카드 수.")]
    [SerializeField] private int stormShakeCount = 3;
    [Tooltip("흔들림 세기.")]
    [SerializeField] private float stormShakeStrength = 0.15f;
    [Tooltip("날아가는 거리.")]
    [SerializeField] private float flyAwayDistance = 40f;
    [Tooltip("날아가는 시간(초).")]
    [SerializeField] private float flyAwayDuration = 0.8f;

    private readonly List<Card> _shakyCards = new List<Card>();
    private readonly Dictionary<Card, Tween> _shakeTweens = new Dictionary<Card, Tween>();

    private readonly Dictionary<Card, GameObject> _frozenIce = new Dictionary<Card, GameObject>();

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;

    /// <summary>자원 채집 ProgressTask가 읽는 전역 속도 배율. 1이면 평소.</summary>
    public static float GatherSpeedMultiplier { get; private set; } = 1f;
    /// <summary>자원 채집 결과가 2배가 될 확률 (0~1). 0이면 항상 1배.</summary>
    public static float GatherDoubleChance { get; private set; } = 0f;

    private int _daysRemaining;

    /// <summary>세이브 저장용 — 현재 날씨 남은 일수.</summary>
    public int WeatherDaysRemaining => _daysRemaining;

    /// <summary>룰렛이 멈춰 결과가 확정된 시점에 호출 (UI는 아직 화면에 떠 있을 수 있음).</summary>
    public event Action<WeatherType> OnWeatherDetermined;
    /// <summary>날씨 지속 기간이 끝나 날씨가 해제될 때 호출.</summary>
    public event Action OnWeatherCleared;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
        UnfreezeAllCards();
        StopAllShakes();
        // static 누수 방지 (씬 재시작 시 평소 상태로)
        GatherSpeedMultiplier = 1f;
        GatherDoubleChance = 0f;
    }

    private void Start()
    {
        if (rouletteGroup != null)
        {
            rouletteGroup.alpha = 0f;
            rouletteGroup.gameObject.SetActive(false);
        }

        ClearWeather();
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += HandleDayChanged;
    }

    /// <summary>
    /// 룰렛을 화면에 표시하고 무작위 날씨로 회전시켜 멈춘 뒤, 잠시 결과를 보여주고 숨김.
    /// onComplete는 룰렛이 완전히 사라진 직후 결과와 함께 호출.
    /// </summary>
    public void ShowRoulette(Action<WeatherType> onComplete)
    {
        if (rouletteGroup == null || wheel == null)
        {
            Debug.LogError("[Weather] rouletteGroup 또는 wheel이 인스펙터에 할당되지 않음. WeatherManager 슬롯을 확인하세요.");
            onComplete?.Invoke(CurrentWeather);
            return;
        }

        WeatherType result = (WeatherType)UnityEngine.Random.Range(0, 4);

        // 휠을 시계방향(CW)으로 X도 돌리면 원래 X도 CW 위치에 있던 칸이 아니라
        // (360 - X)도 CW에 있던 칸이 위(12시)로 온다.
        // 결과 칸은 enum 인덱스 × 90도 CW 위치에 그려져 있으므로,
        // 이를 위로 가져오려면 CW (360 - 인덱스×90)도 회전해야 함.
        float targetSector = ((4 - (int)result) % 4) * 90f;
        float total = extraSpins * 360f + targetSector;

        rouletteGroup.gameObject.SetActive(true);
        rouletteGroup.alpha = 1f;
        wheel.localEulerAngles = Vector3.zero;

        wheel.DOLocalRotate(new Vector3(0f, 0f, -total), spinDuration, RotateMode.FastBeyond360)
             .SetEase(Ease.OutCubic)
             .SetUpdate(true)   // Time.timeScale=0 상태에서도 회전
             .OnComplete(() =>
             {
                 ApplyWeather(result);          // CurrentWeather/배율/이펙트/지속일 세팅
                 OnWeatherDetermined?.Invoke(result);
                 Debug.Log($"[Weather] 룰렛 결과: {result}");

                 // 결과 칸이 보이도록 잠시 머무른 뒤 자동 숨김
                 DOVirtual.DelayedCall(showResultDuration, () =>
                 {
                     HideRoulette();
                     onComplete?.Invoke(result);
                 }, ignoreTimeScale: true);
             });
    }

    private void HideRoulette()
    {
        if (rouletteGroup == null) return;
        rouletteGroup.alpha = 0f;
        rouletteGroup.gameObject.SetActive(false);
    }

    private void ApplyWeather(WeatherType weather)
    {
        CurrentWeather = weather;
        _daysRemaining = weatherDurationDays;

        // 기본값으로 초기화 후 해당 날씨만 적용
        GatherSpeedMultiplier = 1f;
        GatherDoubleChance = 0f;
        if (sunnyEffect != null) sunnyEffect.SetActive(false);
        if (rainEffect != null) rainEffect.SetActive(false);
        if (snowEffect != null) snowEffect.SetActive(false);
        if (stormEffect != null) stormEffect.SetActive(false);

        switch (weather)
        {
            case WeatherType.Sunny:
                if (sunnyEffect != null) sunnyEffect.SetActive(true);
                GatherDoubleChance = sunnyDoubleChance;  // 드랍 2배
                break;

            case WeatherType.Rain:
                if (rainEffect != null) rainEffect.SetActive(true);
                GatherSpeedMultiplier = rainSpeedMultiplier;  // 속도 증가만 유지
                break;

            case WeatherType.Snow:
                if (snowEffect != null) snowEffect.SetActive(true);
                FreezeRandomCards();
                break;

            case WeatherType.Storm:
                if (stormEffect != null) stormEffect.SetActive(true);
                SelectAndShakeCards();
                break;
        }
    }

    /// <summary>
    /// 세이브 복원용 — 룰렛 없이 날씨 상태를 직접 설정.
    /// 배율·이펙트 GameObject는 적용하되, 눈/태풍의 무작위 카드 선택(얼리기/흔들기)은
    /// 재현하지 않음 (얼린/흔든 카드 상태는 저장 대상이 아님).
    /// </summary>
    public void LoadWeather(WeatherType weather, int daysRemaining)
    {
        if (daysRemaining <= 0)
        {
            ClearWeather();
            return;
        }

        CurrentWeather = weather;
        _daysRemaining = daysRemaining;

        GatherSpeedMultiplier = 1f;
        GatherDoubleChance = 0f;
        if (sunnyEffect != null) sunnyEffect.SetActive(false);
        if (rainEffect != null) rainEffect.SetActive(false);
        if (snowEffect != null) snowEffect.SetActive(false);
        if (stormEffect != null) stormEffect.SetActive(false);

        switch (weather)
        {
            case WeatherType.Sunny:
                if (sunnyEffect != null) sunnyEffect.SetActive(true);
                GatherDoubleChance = sunnyDoubleChance;
                break;
            case WeatherType.Rain:
                if (rainEffect != null) rainEffect.SetActive(true);
                GatherSpeedMultiplier = rainSpeedMultiplier;
                break;
            case WeatherType.Snow:
                if (snowEffect != null) snowEffect.SetActive(true);
                break;
            case WeatherType.Storm:
                if (stormEffect != null) stormEffect.SetActive(true);
                break;
        }
    }

    private void ClearWeather()
    {
        CurrentWeather = WeatherType.Sunny;
        GatherSpeedMultiplier = 1f;
        GatherDoubleChance = 0f;
        _daysRemaining = 0;
        if (sunnyEffect != null) sunnyEffect.SetActive(false);
        if (rainEffect != null) rainEffect.SetActive(false);
        if (snowEffect != null) snowEffect.SetActive(false);
        if (stormEffect != null) stormEffect.SetActive(false);
        UnfreezeAllCards();
        StopAllShakes();
        OnWeatherCleared?.Invoke();
    }

    private void HandleDayChanged(int newDay)
    {
        if (_daysRemaining <= 0) return;

        // Storm: 날 바뀔 때 1장 날려보내기
        if (CurrentWeather == WeatherType.Storm && _shakyCards.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, _shakyCards.Count);
            FlyAwayCard(_shakyCards[idx]);
        }

        _daysRemaining--;
        if (_daysRemaining <= 0)
        {
            ClearWeather();
            return;
        }

        // 지속 중 — 눈은 매일 얼리는 카드 갱신
        if (CurrentWeather == WeatherType.Snow)
        {
            UnfreezeAllCards();
            FreezeRandomCards();
        }

        // 지속 중 — 태풍은 새 카드 3장 선택
        if (CurrentWeather == WeatherType.Storm)
        {
            StopAllShakes();
            SelectAndShakeCards();
        }
    }

    // ─────────────────────────────────────────────
    // 눈: 카드 얼리기
    // ─────────────────────────────────────────────

    private void FreezeRandomCards()
    {
        var candidates = new List<Card>();
        foreach (var c in UnityEngine.Object.FindObjectsByType<Card>(FindObjectsSortMode.None))
            if (IsFreezable(c)) candidates.Add(c);

        for (int i = 0; i < freezeCount && candidates.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, candidates.Count);
            var pick = candidates[idx];
            candidates.RemoveAt(idx);
            FreezeCard(pick);
        }
    }

    // 얼릴 수 있는 카드: 음식 / 주민 / 건물 / 채집물(Source)만 (적·팩·자원은 자동 제외)
    private bool IsFreezable(Card c)
    {
        if (c == null || c.IsFrozen) return false;
        if (c.stack == null) return false;
        if (c.stack is BattlePoint) return false;   // 전투 중 카드 제외

        return c.data is FoodCardData
            || c.data is VillagerCardData
            || c.data is BuildingCardData
            || c.data is SourceCardData;
    }

    private void FreezeCard(Card card)
    {
        // 다른 카드와 함께 있으면 단일 분리 → 개별 스택
        if (card.stack != null && card.stack.cards.Count > 1 && cardStackPrefab != null)
        {
            Vector3 worldPos = card.transform.position;
            card.stack.SplitSingleCard(card);   // detach (stack=null, parent=null)

            var go = Instantiate(cardStackPrefab, worldPos, Quaternion.identity);
            var newStack = go.GetComponent<CardStack>();
            if (newStack != null) newStack.AddCard(card);
        }

        card.IsFrozen = true;

        GameObject ice = null;
        if (icePrefab != null)
            ice = Instantiate(icePrefab, card.transform.position, Quaternion.identity, card.transform);
        _frozenIce[card] = ice;
    }

    private void UnfreezeAllCards()
    {
        foreach (var kv in _frozenIce)
        {
            if (kv.Key != null) kv.Key.IsFrozen = false;
            if (kv.Value != null) Destroy(kv.Value);
        }
        _frozenIce.Clear();
    }

    // ─────────────────────────────────────────────
    // 태풍: 카드 흔들기 / 날려보내기
    // ─────────────────────────────────────────────

    private void SelectAndShakeCards()
    {
        var candidates = new List<Card>();
        foreach (var c in UnityEngine.Object.FindObjectsByType<Card>(FindObjectsSortMode.None))
            if (IsShakeable(c)) candidates.Add(c);

        for (int i = 0; i < stormShakeCount && candidates.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, candidates.Count);
            var pick = candidates[idx];
            candidates.RemoveAt(idx);

            pick.suppressFollow = true;
            var tween = pick.transform
                .DOShakePosition(1f, new Vector3(stormShakeStrength, 0f, stormShakeStrength), 15, 90f, false, true)
                .SetLoops(-1)
                .SetUpdate(true);

            _shakeTweens[pick] = tween;
            _shakyCards.Add(pick);
        }
    }

    private void StopAllShakes()
    {
        foreach (var kv in _shakeTweens)
        {
            kv.Value?.Kill();
            if (kv.Key != null)
            {
                kv.Key.suppressFollow = false;
                kv.Key.transform.localPosition = kv.Key.targetLocalPosition; // 원위치 복귀
            }
        }
        _shakeTweens.Clear();
        _shakyCards.Clear();
    }

    private void FlyAwayCard(Card card)
    {
        // 해당 카드의 모든 DOTween 트윈 즉시 중단 (shake 포함, 트윈 충돌 방지)
        DOTween.Kill(card.transform);
        _shakeTweens.Remove(card);
        _shakyCards.Remove(card);

        // 스택에서 올바르게 분리
        var stack = card.stack;
        if (stack != null)
        {
            if (stack.cards.Count > 1)
            {
                // SplitSingleCard: card.stack=null, SetParent(null), ArrangeCards() 포함
                stack.SplitSingleCard(card);
            }
            else
            {
                // 혼자 있는 스택 → 스택 자체를 제거
                stack.cards.Remove(card);
                card.stack = null;
                card.transform.SetParent(null);
                Destroy(stack.gameObject);
            }
        }

        // SplitSingleCard가 isKinematic=false로 설정하므로, DOMove를 위해 다시 kinematic 고정
        if (card.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

        // 날아가기 이펙트: 카드 원래 위치에 스폰 (부모 없음, 파티클 자체적으로 Destroy 처리)
        if (flyAwayEffectPrefab != null)
            Instantiate(flyAwayEffectPrefab, card.transform.position, Quaternion.identity);

        // 랜덤 상방으로 날려보내기
        card.suppressFollow = true;
        Vector3 dir = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            1f,
            UnityEngine.Random.Range(-0.5f, 0.5f)
        ).normalized;
        Vector3 target = card.transform.position + dir * flyAwayDistance;

        card.transform
            .DOMove(target, flyAwayDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)   // DayChangeOverlay(timeScale=0) 중에도 동작
            .OnComplete(() =>
            {
                if (card != null) Destroy(card.gameObject);
            });
    }

    private bool IsShakeable(Card c)
    {
        if (c == null || c.IsFrozen) return false;
        if (c.stack == null) return false;
        if (_shakyCards.Contains(c)) return false;  // 이미 흔들리는 카드 제외

        return c.data is ResourceCardData
            || c.data is SourceCardData
            || c.data is FoodCardData;
    }
}
