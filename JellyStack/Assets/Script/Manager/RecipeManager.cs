using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [Header("등록된 레시피 목록")]
    [SerializeField] private List<CardRecipe> recipes = new List<CardRecipe>();

    [Header("힐 시스템")]
    [Tooltip("Villager + Hearts 스택에서 사용할 진행/이펙트 레시피 (ingredients/result 비워둘 것)")]
    [SerializeField] private CardRecipe healRecipe;

    private CardRecipe pendingRecipe;
    private float pendingElapsed;
    private int pendingFrame = -1;

    private void Awake()
    {
        Instance = this;
    }

    public void StageTransfer(CardRecipe recipe, float elapsed)
    {
        pendingRecipe = recipe;
        pendingElapsed = elapsed;
        pendingFrame = Time.frameCount;
    }

    /// <summary>
    /// 카드 데이터가 레시피 ingredient와 매칭되는지 판정.
    /// 와일드카드 규칙: ingredient가 VillagerCardData이면, isBaby=false인 어떤 VillagerCardData든 매칭.
    /// 그 외(House/Tree/Berry 등)는 reference 정확 일치.
    /// </summary>
    private static bool IsIngredientMatch(CardData cardData, CardData ingredient)
    {
        if (cardData == null || ingredient == null) return false;

        // Villager 와일드카드: ingredient가 어떤 VillagerCardData이든 → 일하는 Villager만 매칭
        if (ingredient is VillagerCardData)
        {
            return cardData is VillagerCardData vd && !vd.isBaby;
        }

        // 그 외: 데이터 참조 정확 일치
        return cardData == ingredient;
    }

    /// <summary>data 리스트에서 ingredient와 매칭되는 첫 인덱스 반환. 없으면 -1.</summary>
    private static int FindIngredientIndex(List<CardData> data, CardData ingredient)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (IsIngredientMatch(data[i], ingredient)) return i;
        }
        return -1;
    }

    public bool CardsMatchIngredients(List<Card> cards, List<CardData> ingredients)
    {
        if (cards == null || ingredients == null || ingredients.Count == 0) return false;
        var data = new List<CardData>(cards.Count);
        foreach (var c in cards) data.Add(c.data);
        foreach (var ing in ingredients)
        {
            int idx = FindIngredientIndex(data, ing);
            if (idx < 0) return false;
            data.RemoveAt(idx);
        }
        return true;
    }

    public void CheckStack(CardStack stack)
    {
        if (stack == null || stack.cards.Count < 2)
            return;

        // 얼어붙은 카드가 포함된 스택은 작업/힐 시작 불가 (눈 날씨)
        foreach (var c in stack.cards)
            if (c != null && c.IsFrozen) return;

        var existingTask = stack.GetComponent<ProgressTask>();
        if (existingTask != null && existingTask.enabled)
            return;

        CardRecipe matched = FindMatchingRecipe(stack);
        if (matched != null)
        {
            float startElapsed = 0f;
            if (pendingRecipe == matched && pendingFrame == Time.frameCount)
            {
                startElapsed = pendingElapsed;
                pendingRecipe = null;
                pendingFrame = -1;
            }
            var task = stack.gameObject.AddComponent<ProgressTask>();
            task.Begin(matched, stack, OnRecipeComplete, startElapsed);
            return;
        }

        // 일반 레시피가 안 잡히면 힐 패턴 시도
        TryStartHealTask(stack);
    }

    private bool TryStartHealTask(CardStack stack)
    {
        if (healRecipe == null) return false;
        if (stack == null || stack.cards.Count < 2) return false;

        VillagerCard villager = null;
        int heartCount = 0;
        foreach (var c in stack.cards)
        {
            if (c is VillagerCard v)
            {
                if (villager != null) return false; // Villager 2명 이상은 패턴 아님
                villager = v;
            }
            else if (c is HeartCard)
            {
                heartCount++;
            }
            else
            {
                return false; // 다른 카드 섞임 → 엄격 패턴 위반
            }
        }

        if (villager == null || heartCount == 0) return false;
        if (villager.CurrentHealth >= villager.MaxHealth) return false; // 이미 풀체력

        float startElapsed = 0f;
        if (pendingRecipe == healRecipe && pendingFrame == Time.frameCount)
        {
            startElapsed = pendingElapsed;
            pendingRecipe = null;
            pendingFrame = -1;
        }

        var task = stack.gameObject.AddComponent<ProgressTask>();
        task.Begin(healRecipe, stack, OnRecipeComplete, startElapsed);
        return true;
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

    public bool StackMatchesIngredients(CardStack stack, List<CardData> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0) return false;
        var stackData = new List<CardData>(stack.cards.Select(c => c.data));
        foreach (var ingredient in ingredients)
        {
            int idx = FindIngredientIndex(stackData, ingredient);
            if (idx < 0) return false;
            stackData.RemoveAt(idx);
        }
        return true;
    }

    private void OnRecipeComplete(CardRecipe recipe, CardStack stack)
    {
        Debug.Log($"레시피 완료: {recipe.name}");

        if (recipe == healRecipe)
        {
            ApplyHeal(stack);
        }
        else
        {
            if (recipe.result != null)
                SpawnResults(recipe, stack);

            HandleIngredients(recipe, stack);
        }

        if (stack != null && !stack.IsEmpty)
            CheckStack(stack);
    }

    private void ApplyHeal(CardStack stack)
    {
        if (stack == null) return;

        VillagerCard villager = null;
        var hearts = new List<HeartCard>();
        foreach (var c in stack.cards)
        {
            if (c is VillagerCard v) villager = v;
            else if (c is HeartCard h) hearts.Add(h);
        }
        if (villager == null || hearts.Count == 0) return;

        int totalHeal = 0;
        foreach (var h in hearts) totalHeal += h.HealAmount;
        villager.Heal(totalHeal);

        // Hearts 제거 및 파괴
        foreach (var h in hearts)
        {
            stack.cards.Remove(h);
            Destroy(h.gameObject);
        }
        stack.Refresh();
    }

    private void SpawnResults(CardRecipe recipe, CardStack stack)
    {
        if (CardSpawner.Instance == null) return;

        Vector3 sourcePos = stack.transform.position;

        int count = recipe.resultCount;
        // 자원 채집(스택에 SourceCard 포함) + 날씨 더블 확률 발동 시 2배
        if (StackHasSource(stack) &&
            UnityEngine.Random.value < WeatherManager.GatherDoubleChance)
        {
            count *= 2;
            Debug.Log("[Weather] 채집 2배 발동!");
        }

        for (int i = 0; i < count; i++)
            CardSpawner.Instance.SpawnNear(recipe.result, sourcePos, stack);
    }

    private bool StackHasSource(CardStack stack)
    {
        foreach (var c in stack.cards)
            if (c is SourceCard) return true;
        return false;
    }

    private void HandleIngredients(CardRecipe recipe, CardStack stack)
    {
        var alreadyProcessed = new HashSet<Card>();

        foreach (var ingredient in recipe.ingredients)
        {
            Card found = stack.cards.Find(c => IsIngredientMatch(c.data, ingredient) && !alreadyProcessed.Contains(c));
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
