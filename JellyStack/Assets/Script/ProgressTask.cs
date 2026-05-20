using UnityEngine;

public class ProgressTask : MonoBehaviour
{
    private CardRecipe recipe;
    private CardStack stack;
    private float elapsed;
    private System.Action<CardRecipe, CardStack> onComplete;

    private bool isCompleted;

    public void Begin(CardRecipe recipe, CardStack stack, System.Action<CardRecipe, CardStack> onComplete)
    {
        this.recipe = recipe;
        this.stack = stack;
        this.elapsed = 0f;
        this.onComplete = onComplete;
        this.isCompleted = false;

        if (stack != null && stack.ProgressBar != null)
        {
            stack.ProgressBar.Show();
            stack.ProgressBar.SetProgress(0f);
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
        if (stack.ProgressBar != null )
            stack.ProgressBar.SetProgress(progress);

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
