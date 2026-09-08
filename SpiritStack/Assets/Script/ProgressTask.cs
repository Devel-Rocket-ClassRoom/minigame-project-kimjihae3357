using System.Collections.Generic;
using UnityEngine;

public class ProgressTask : MonoBehaviour
{
    private CardRecipe recipe;
    public CardRecipe Recipe => recipe;
    private CardStack stack;
    private float elapsed;
    public float Elapsed => elapsed;
    private float _durationOverride = -1f;
    private System.Action<CardRecipe, CardStack> onComplete;

    private bool isCompleted;

    /// <summary>FeedTime 중 모든 ProgressTask 일시정지 플래그</summary>
    public static bool IsPaused = false;

    private GameObject _activeEffect;

    // 작업 시작 시점 stack.cards 스냅샷. 매 프레임 이 카드들이 stack에 그대로 있는지 검증해
    // 판매/사망/분리 등 어떤 경로로든 재료가 빠지면 즉시 Cancel.
    // (RecipeManager.CheckStack은 정확 일치일 때만 task를 시작하므로 시작 시점 stack.cards = 레시피 재료.)
    private List<Card> _ingredientSnapshot;

    public void Begin(CardRecipe recipe, CardStack stack, System.Action<CardRecipe, CardStack> onComplete, float startElapsed = 0f, float durationOverride = -1f, GameObject effectOverride = null)
    {
        this.recipe = recipe;
        this.stack = stack;
        this.elapsed = startElapsed;
        this.onComplete = onComplete;
        this.isCompleted = false;
        this._durationOverride = durationOverride;

        if (stack != null && stack.ProgressBar != null)
        {
            stack.ProgressBar.Show();
            float duration = Mathf.Max(0.01f, _durationOverride > 0f ? _durationOverride : recipe.duration);
            stack.ProgressBar.SetProgress(startElapsed / duration);
        }

        // 진행 이펙트 스폰 (스택 자식으로 부모 설정 → 카드 이동을 따라감)
        // effectOverride가 지정되면 우선 사용(예: SourceCardData별 채집 이펙트), 없으면 레시피 이펙트.
        GameObject effectToSpawn = effectOverride != null ? effectOverride : (recipe != null ? recipe.effectPrefab : null);
        if (effectToSpawn != null && stack != null)
        {
            Vector3 spawnPos = stack.TopCard != null
                ? stack.TopCard.transform.position
                : stack.transform.position;

            _activeEffect = Instantiate(
                effectToSpawn,
                spawnPos,
                Quaternion.identity,
                stack.transform
            );
        }

        // 시작 시점 재료 카드 스냅샷 — 이후 Update에서 매 프레임 검증해 재료가 빠지면 Cancel.
        _ingredientSnapshot = stack != null ? new List<Card>(stack.cards) : null;
    }

    private void Update()
    {
        if (isCompleted) return;
        if (IsPaused) return;  // FeedTime 중 정지

        if (stack == null || recipe == null)
        {
            Cancel();
            return;
        }

        // 시작 시점 재료 카드 중 하나라도 사라지거나 stack 밖이면 작업 중단.
        // (판매/사망/머지/분리 등 모든 카드 제거 경로를 ProgressTask 자체에서 한 곳에 커버.)
        if (_ingredientSnapshot != null)
        {
            for (int i = 0; i < _ingredientSnapshot.Count; i++)
            {
                var c = _ingredientSnapshot[i];
                if (c == null || !stack.cards.Contains(c))
                {
                    Cancel();
                    return;
                }
            }
        }

        // 시간 진행
        elapsed += Time.deltaTime;
        float effectiveDuration = Mathf.Max(0.01f, _durationOverride > 0f ? _durationOverride : recipe.duration);

        bool hasSource = false;
        bool workSpeedApplied = false;
        foreach (var card in stack.cards)
        {
            if (!workSpeedApplied && card.data is VillagerCardData villagerData)
            {
                effectiveDuration /= Mathf.Max(0.01f, villagerData.workSpeed);
                workSpeedApplied = true;
            }
            if (card is SourceCard)
                hasSource = true;
        }

        // 자원 채집(스택에 SourceCard 포함) 작업에만 날씨 배율 적용
        if (hasSource)
            effectiveDuration /= Mathf.Max(0.01f, WeatherManager.GatherSpeedMultiplier);

        float progress = elapsed / effectiveDuration;

        // ProgressBar 갱신
        if (stack.ProgressBar != null)
        {
            stack.ProgressBar.Show();
            stack.ProgressBar.SetProgress(progress);
        }

        // 완료 체크
        if (elapsed >= effectiveDuration)
            Complete();
    }

    private void Complete()
    {
        if (isCompleted) return;
        isCompleted = true;

        enabled = false;
        HideProgress();
        DestroyActiveEffect();

        try
        {
            onComplete?.Invoke(recipe, stack);
        }
        finally
        {
            Destroy(this);
        }
    }

    public void Cancel()
    {
        HideProgress();
        DestroyActiveEffect();
        Destroy(this);
    }

    private void HideProgress()
    {
        if (stack != null && stack.ProgressBar != null)
            stack.ProgressBar.Hide();
    }

    private void DestroyActiveEffect()
    {
        if (_activeEffect != null)
        {
            Destroy(_activeEffect);
            _activeEffect = null;
        }
    }

    private void OnDestroy()
    {
        // 어떤 경로로 컴포넌트가 파괴되더라도 잔여 이펙트가 남지 않도록
        DestroyActiveEffect();
    }
}
