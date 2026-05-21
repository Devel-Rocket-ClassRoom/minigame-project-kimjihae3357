using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [Header("등록된 레시피 목록")]
    [SerializeField] private List<CardRecipe> recipes = new List<CardRecipe>();

    private void Awake()
    {
        Instance = this;
    }

    public void CheckStack(CardStack stack)
    {
        if (stack == null || stack.cards.Count < 2)
            return;
        var existingTask = stack.GetComponent<ProgressTask>();
        if (existingTask != null && existingTask.enabled)
            return;

        CardRecipe matched = FindMatchingRecipe(stack);
        if (matched != null)
        {
            var task = stack.gameObject.AddComponent<ProgressTask>();
            task.Begin(matched, stack, OnRecipeComplete);
        }
    }

    private CardRecipe FindMatchingRecipe(CardStack stack)
    {
        foreach (var recipe in recipes)
        {
            if (StackMatchesIngredients(stack, recipe.ingredients))
                return recipe;
        }
        return null;
    }

    private bool StackMatchesIngredients(CardStack stack, List<CardData> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0) return false;
        var stackData = new List<CardData>(stack.cards.Select(c => c.data));
        foreach (var ingredient in ingredients)
        {
            int idx = stackData.IndexOf(ingredient);
            if (idx < 0) return false;
            stackData.RemoveAt(idx);
        }
        return true;
    }

    private void OnRecipeComplete(CardRecipe recipe, CardStack stack)
    {
        Debug.Log($"레시피 완료: {recipe.name}");

        if (recipe.result != null)
            SpawnResults(recipe, stack);

        HandleIngredients(recipe, stack);

        if (stack != null && !stack.IsEmpty)
            CheckStack(stack);
    }

    private void SpawnResults(CardRecipe recipe, CardStack stack)
    {
        if (CardSpawner.Instance == null) return;

        Vector3 sourcePos = stack.transform.position;

        for (int i = 0; i < recipe.resultCount; i++)
            CardSpawner.Instance.SpawnNear(recipe.result, sourcePos, stack);
    }

    private void HandleIngredients(CardRecipe recipe, CardStack stack)
    {
        var alreadyProcessed = new HashSet<Card>();

        foreach (var ingredient in recipe.ingredients)
        {
            Card found = stack.cards.Find(c => c.data == ingredient && !alreadyProcessed.Contains(c));
            if (found == null) continue;

            alreadyProcessed.Add(found);

            if (found is SourceCard source)
            {
                source.Gather();
                if (source.IsExhausted)
                {
                    stack.cards.Remove(found);
                    Destroy(found.gameObject);
                }
            }
            else if (recipe.consumeIngredients)
            {
                stack.cards.Remove(found);
                Destroy(found.gameObject);
            }
        }

        stack.Refresh();
    }
}
