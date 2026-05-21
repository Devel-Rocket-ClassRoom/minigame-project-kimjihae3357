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
    }

    private void Update()
    {
        if (isCompleted) return;

        if (stack == null || recipe == null)
        {
            Cancel();
            return;
        }

        // 시간 진행
        elapsed += Time.deltaTime;
        float effectiveDuration = Mathf.Max(0.01f, recipe.duration);
        foreach (var card in stack.cards)
        {
            if (card.data is VillagerCardData villagerData)
            {
                effectiveDuration /= Mathf.Max(0.01f, villagerData.workSpeed);
                break;
            }
        }

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
        Destroy(this);
    }

    private void HideProgress()
    {
        if (stack != null && stack.ProgressBar != null)
            stack.ProgressBar.Hide();
    }
}
