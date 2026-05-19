using UnityEngine;

public class ProgressTask : MonoBehaviour
{
    private CardRecipe recipe;
    private CardStack stack;
    private float elapsed;
    private System.Action<CardRecipe, CardStack> onComplete;

    public void Begin(CardRecipe recipe, CardStack stack, System.Action<CardRecipe, CardStack> onComplete)
    {
        this.recipe = recipe;
        this.stack = stack;
        this.elapsed = 0f;
        this.onComplete = onComplete;

        if (stack != null && stack.ProgressBar != null)
        {
            stack.ProgressBar.Show();
            stack.ProgressBar.SetProgress(0f);
        }
    }

    private void Update()
    {
        if (stack == null || recipe == null)
        {
            Cancel();
            return;
        }

        // 시간 진행
        elapsed += Time.deltaTime;
        float progress = elapsed / recipe.duration;

        // ProgressBar 갱신
        if (stack.ProgressBar != null )
            stack.ProgressBar.SetProgress(progress);

        // 완료 체크
        if (elapsed >= recipe.duration)
            Complete();
    }

    private void Complete()
    {
        enabled = false;
        HideProgress();
        onComplete?.Invoke(recipe, stack);
        Destroy(this);
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
