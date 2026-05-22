using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// FeedPhase 매니저.
/// Day 타이머 만료 시 게임을 멈추고 플레이어가 Food 카드를 직접 선택해 Villager에게 배분.
/// 모든 Villager hunger가 0이 되면 DayChange로 진행, Food 소진 시 GameOver.
/// </summary>
public class FeedManager : MonoBehaviour
{
    public static FeedManager Instance { get; private set; }

    [Header("프리팹")]
    [SerializeField] private GameObject selectCardPrefab;

    [Header("애니메이션")]
    [SerializeField] private float moveDuration = 0.45f;

    private System.Action _onFeedComplete;
    private readonly List<SelectFoodCard> _activeIndicators = new();
    private bool _isAnimating;

    private void Awake() => Instance = this;

    private void Start()
    {
        DayManager.Instance.OnBeforeDayChanged += HandleBeforeDayChanged;
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnBeforeDayChanged -= HandleBeforeDayChanged;
    }

    // ─────────────────────────────────────────────
    // FeedPhase 진입
    // ─────────────────────────────────────────────

    private void HandleBeforeDayChanged(System.Action onComplete)
    {
        _onFeedComplete = onComplete;
        _isAnimating = false;

        InputManager.IsBlocked = true;
        ProgressTask.IsPaused = true;   // 채집 진행 정지

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.ShowFeedOverlay();

        var foods = Object.FindObjectsByType<FoodCard>(FindObjectsSortMode.None)
                          .Where(f => f.CurrentFullness > 0)
                          .ToArray();

        if (foods.Length == 0)
        {
            CheckFeedCompletion();
            return;
        }

        foreach (var food in foods)
            SpawnIndicator(food);
    }

    // ─────────────────────────────────────────────
    // SelectFoodCard 인디케이터 생성
    // ─────────────────────────────────────────────

    private void SpawnIndicator(FoodCard food)
    {
        if (selectCardPrefab == null) return;

        var go = Instantiate(selectCardPrefab);
        var indicator = go.GetComponent<SelectFoodCard>();
        if (indicator == null)
        {
            Debug.LogError("SelectCard 프리팹에 SelectFoodCard 컴포넌트가 없습니다.");
            Destroy(go);
            return;
        }

        indicator.Setup(food, this);
        _activeIndicators.Add(indicator);
    }

    // ─────────────────────────────────────────────
    // 플레이어가 Food 카드 선택 시
    // ─────────────────────────────────────────────

    public void OnFoodSelected(FoodCard food, SelectFoodCard indicator)
    {
        if (_isAnimating) return;
        if (food == null) return;

        _activeIndicators.Remove(indicator);
        Destroy(indicator.gameObject);

        var target = FindHungriestVillager();
        if (target == null)
        {
            CheckFeedCompletion();
            return;
        }

        int amount = Mathf.Min(food.CurrentFullness, target.Currenthunger);
        _isAnimating = true;
        food.suppressFollow = true;

        food.transform
            .DOMove(target.transform.position, moveDuration)
            .SetUpdate(true)          // timeScale=0에서도 동작
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                if (food != null)
                {
                    food.Consume(amount);

                    if (food.CurrentFullness <= 0)
                    {
                        food.stack?.cards.Remove(food);
                        Destroy(food.gameObject);
                    }
                    else
                    {
                        food.suppressFollow = false;
                        SpawnIndicator(food);   // 잔량 있으면 재선택 가능
                    }
                }

                if (target != null)
                    target.Feed(amount);

                _isAnimating = false;
                CheckFeedCompletion();
            });
    }

    // ─────────────────────────────────────────────
    // 완료 조건 확인
    // ─────────────────────────────────────────────

    private void CheckFeedCompletion()
    {
        var villagers = Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None);

        bool allFed = villagers.Length == 0 ||
                      villagers.All(v => v.Currenthunger == 0);

        if (allFed)
        {
            FinishFeedPhase(success: true);
            return;
        }

        bool noFood = !Object.FindObjectsByType<FoodCard>(FindObjectsSortMode.None)
                             .Any(f => f.CurrentFullness > 0);

        if (noFood)
            FinishFeedPhase(success: false);

        // 아직 Food가 남아있으면 플레이어 선택 대기
    }

    // ─────────────────────────────────────────────
    // FeedPhase 종료
    // ─────────────────────────────────────────────

    private void FinishFeedPhase(bool success)
    {
        // 남은 인디케이터 전부 제거
        foreach (var ind in _activeIndicators)
            if (ind != null) Destroy(ind.gameObject);
        _activeIndicators.Clear();

        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.HideFeedOverlay();

        InputManager.IsBlocked = false;
        ProgressTask.IsPaused = false;  // 채집 진행 재개

        if (success)
        {
            // 모든 Villager hunger 리셋
            foreach (var v in Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None))
                v.ResetHunger();

            _onFeedComplete?.Invoke();   // → ContinueDayChange → DayChangeOverlay
        }
        else
        {
            // 음식 부족 → GameOver
            GameManager.Instance?.GameOver();
        }
    }

    // ─────────────────────────────────────────────
    // 헬퍼
    // ─────────────────────────────────────────────

    private VillagerCard FindHungriestVillager()
    {
        VillagerCard hungriest = null;
        int maxHunger = 0;

        foreach (var v in Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None))
        {
            if (v.Currenthunger > maxHunger)
            {
                maxHunger = v.Currenthunger;
                hungriest = v;
            }
        }

        return hungriest;
    }
}
