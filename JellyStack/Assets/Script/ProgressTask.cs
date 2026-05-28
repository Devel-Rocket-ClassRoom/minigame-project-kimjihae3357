using UnityEngine;

public class ProgressTask : MonoBehaviour
{
    private CardRecipe recipe;
    public CardRecipe Recipe => recipe;
    private CardStack stack;
    private float elapsed;
    public float Elapsed => elapsed;
    private System.Action<CardRecipe, CardStack> onComplete;

    private bool isCompleted;

    /// <summary>FeedTime 중 모든 ProgressTask 일시정지 플래그</summary>
    public static bool IsPaused = false;

    private GameObject _activeEffect;

    public void Begin(CardRecipe recipe, CardStack stack, System.Action<CardRecipe, CardStack> onComplete, float startElapsed = 0f)
    {
        this.recipe = recipe;
        this.stack = stack;
        this.elapsed = startElapsed;
        this.onComplete = onComplete;
        this.isCompleted = false;

        if (stack != null && stack.ProgressBar != null)
        {
            stack.ProgressBar.Show();
            float duration = Mathf.Max(0.01f, recipe.duration);
            stack.ProgressBar.SetProgress(startElapsed / duration);
        }

        // 진행 이펙트 스폰 (스택 자식으로 부모 설정 → 카드 이동을 따라감)
        if (recipe != null && recipe.progressEffectPrefab != null && stack != null)
        {
            Vector3 spawnPos = stack.TopCard != null
                ? stack.TopCard.transform.position
                : stack.transform.position;

            _activeEffect = Instantiate(
                recipe.progressEffectPrefab,
                spawnPos,
                Quaternion.identity,
                stack.transform
            );
        }
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

        // 시간 진행
        elapsed += Time.deltaTime;
        float effectiveDuration = Mathf.Max(0.01f, recipe.duration);

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
