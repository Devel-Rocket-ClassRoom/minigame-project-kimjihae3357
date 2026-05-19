using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{

    public static RecipeManager Instance { get; private set; }

    [Header("등록된 레시피 목록")]
    [SerializeField] private List<CardRecipe> recipes = new List<CardRecipe>();

    [Header("결과물 스폰 위치 오프셋")]
    [SerializeField] private Vector3 resultSpawnOffset = new Vector3(2f, 0, 0);

    private void Awake()
    {
        Instance = this;
    }

    public void CheckStack(CardStack stack)
    {
        if (stack == null || stack.cards.Count < 2)
            return;
        if (stack.GetComponent<ProgressTask>() != null)
            return;

        CardRecipe matched = FindMatchingRecipe(stack);
        if (matched != null )
        {
            var task = stack.gameObject.AddComponent<ProgressTask>();
            task.Begin(matched, stack, OnRecipeComplete);
        }
    }

    // 스택과 일치하는 첫 번째 레시피 찾기
    private CardRecipe FindMatchingRecipe(CardStack stack)
    {
        foreach (var recipe in recipes)
        {
            if (StackMatchesIngredients(stack, recipe.ingredients))
                return recipe;
        }
        return null;
    }

    // 스택이 재료 리스트를 포함하는지 검사 (순서 무관)
    private bool StackMatchesIngredients(CardStack stack, List<CardData> ingredients)
    {
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

        // 1. 결과물 스폰 (consumeIngredients와 무관)
        if (recipe.result != null)
            SpawnResults(recipe, stack);   // ← SpawnResult가 아니라 SpawnResults (s 붙음)

        // 2. 재료 처리 (consumeIngredients는 HandleIngredients 안에서 검사)
        HandleIngredients(recipe, stack);

        // 3. 연쇄 작업 검사
        if (stack != null && !stack.IsEmpty)
            CheckStack(stack);
    }

    private void HandleIngredients(CardRecipe recipe, CardStack stack)
    {
        // 매칭된 카드를 추적해서 중복 처리 방지
        var alreadyProcessed = new HashSet<Card>();

        foreach (var ingredient in recipe.ingredients)
        {
            Card found = stack.cards.Find(c => c.data == ingredient && !alreadyProcessed.Contains(c));
            if (found == null) continue;

            alreadyProcessed.Add(found);

            // SourceCard라면 채집 카운트 감소
            if (found is SourceCard source)
            {
                source.Gather();
                if (source.IsExhausted)
                {
                    stack.cards.Remove(found);
                    Destroy(found.gameObject);
                }
            }
            // 일반 카드는 옵션에 따라 소비
            else if (recipe.consumeIngredients)
            {
                stack.cards.Remove(found);
                Destroy(found.gameObject);
            }
            // 그 외 (주민 등 도구 역할의 카드)는 그대로 둠
        }

        stack.Refresh();
    }



    private void SpawnResults(CardRecipe recipe, CardStack stack)
    {
        if(CardSpawner.Instance == null) return;

        for (int i = 0; i < recipe.resultCount; i++)
        {
            Vector3 spawnPos = stack.transform.position
                + resultSpawnOffset
                + new Vector3(i * 0.5f, 0, 0);

            CardSpawner.Instance.Spawn(recipe.result, spawnPos);
        }
    }
}
