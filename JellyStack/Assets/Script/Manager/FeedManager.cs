using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float moveDuration = 0.55f;
    [SerializeField] private float jumpPower = 2f;

    [Header("[연출]")]
    [SerializeField] private GameObject deadEffect;
    [SerializeField] private float deadEffectDuration = 1.0f; // 사망 이펙트 재생 대기 시간(초)
    [SerializeField] private GameObject eatEffect;            // 음식 도착 시 과자 부스러기 이펙트

    private System.Action _onFeedComplete;
    private readonly List<SelectFoodCard> _activeIndicators = new();
    private bool _isAnimating;

    private void Awake()
    {
        Instance = this;
    }


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
            .DOJump(target.transform.position, jumpPower, 1, moveDuration)
            .SetUpdate(true)          // timeScale=0에서도 동작
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 음식이 Villager에 도착한 순간 - 과자 부스러기 이펙트 재생
                if (eatEffect != null)
                {
                    Vector3 spawnPos = target != null
                        ? target.transform.position
                        : (food != null ? food.transform.position : Vector3.zero);
                    Instantiate(eatEffect, spawnPos, Quaternion.identity);
                }

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
            // 모든 Villager hunger 리셋 후 다음 날로
            foreach (var v in Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None))
                v.ResetHunger();

            _onFeedComplete?.Invoke();   // → ContinueDayChange → DayChangeOverlay
        }
        else
        {
            // 음식 부족 → 굶은 Villager 사망 처리 + 연출 대기 후 다음 단계로
            StartCoroutine(DieAndFinish());
        }
    }

    // ─────────────────────────────────────────────
    // 사망 연출 + 후처리 코루틴
    // ─────────────────────────────────────────────

    private IEnumerator DieAndFinish()
    {
        var allVillagers = Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None);
        var survivors = new List<VillagerCard>();
        bool anyDied = false;

        foreach (var v in allVillagers)
        {
            if (v.Currenthunger > 0)
            {
                // 굶어 죽음 - 죽는 카드 위치에서 이펙트 재생
                if (deadEffect != null)
                    Instantiate(deadEffect, v.transform.position, Quaternion.identity);

                v.stack?.cards.Remove(v);
                Destroy(v.gameObject);
                anyDied = true;
            }
            else
            {
                survivors.Add(v);
            }
        }

        // 누군가 죽었다면 연출 시간만큼 대기 (timeScale 영향 없도록 Realtime 사용)
        if (anyDied)
            yield return new WaitForSecondsRealtime(deadEffectDuration);

        if (survivors.Count == 0)
        {
            // 살아남은 주민 없음 → 게임오버
            GameManager.Instance?.GameOver();
        }
        else
        {
            // 생존자 hunger 리셋 후 다음 날로
            foreach (var v in survivors)
                v.ResetHunger();

            _onFeedComplete?.Invoke();
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
