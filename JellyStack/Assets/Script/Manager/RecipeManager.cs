using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }
    public IReadOnlyList<CardRecipe> Recipes => recipes;

    [Header("등록된 레시피 목록")]
    [SerializeField] private List<CardRecipe> recipes = new List<CardRecipe>();

    [Header("힐 시스템")]
    [Tooltip("Villager + Hearts 스택에서 사용할 진행/이펙트 레시피 (ingredients/result 비워둘 것)")]
    [SerializeField] private CardRecipe healRecipe;

    [Header("채집 시스템")]
    [Tooltip("채집 진행/이펙트용 레시피 템플릿 (ingredients/result 비워둘 것). 실제 결과물은 SourceCardData에서 지정.")]
    [SerializeField] private CardRecipe gatherRecipe;

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

        // 채집 패턴을 가장 먼저 확인 (Villager + SourceCard 전용)
        if (TryStartGatherTask(stack)) return;

        // 힐 패턴을 일반 레시피보다 먼저 확인
        // (Villager를 재료로 쓰는 레시피가 [Villager+Heart] 스택을 가로채는 것을 방지)
        if (TryStartHealTask(stack)) return;

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
    }

    private bool TryStartGatherTask(CardStack stack)
    {
        if (gatherRecipe == null) return false;
        if (stack == null || stack.cards.Count < 2) return false;

        bool hasVillager = false;
        SourceCard source = null;

        // 스택이 VillagerCard + SourceCard 로만 구성되는지 검사
        // (다른 카드 타입이 섞이면 패턴 아님 → 일반 레시피에 넘김)
        foreach (var c in stack.cards)
        {
            if (c is VillagerCard v)
            {
                var vd = v.data as VillagerCardData;
                if (vd == null || vd.isBaby) return false; // 아기 주민은 채집 불가
                hasVillager = true;
            }
            else if (c is SourceCard s)
            {
                // 여러 SourceCard → 첫 번째 유효한 것만 이번 사이클에 사용
                if (source == null) source = s;
            }
            else
            {
                return false; // 다른 카드 타입 섞임 → 채집 패턴 아님
            }
        }

        if (!hasVillager || source == null) return false;

        var sourceData = source.data as SourceCardData;
        if (sourceData == null || !HasGatherResults(sourceData)) return false;

        float duration = sourceData.gatherDuration > 0f
            ? sourceData.gatherDuration
            : gatherRecipe.duration;

        float startElapsed = 0f;
        if (pendingRecipe == gatherRecipe && pendingFrame == Time.frameCount)
        {
            startElapsed = pendingElapsed;
            pendingRecipe = null;
            pendingFrame = -1;
        }

        var task = stack.gameObject.AddComponent<ProgressTask>();
        task.Begin(gatherRecipe, stack, OnGatherComplete, startElapsed, duration, sourceData.gatherEffectPrefab);
        return true;
    }

    private void OnGatherComplete(CardRecipe recipe, CardStack stack)
    {
        if (stack == null) return;

        SourceCard source = null;
        foreach (var c in stack.cards)
        {
            if (c is SourceCard s) { source = s; break; }
        }
        if (source == null) { if (!stack.IsEmpty) CheckStack(stack); return; }

        var sourceData = source.data as SourceCardData;
        if (sourceData == null)
        {
            if (!stack.IsEmpty) CheckStack(stack);
            return;
        }

        // 랜덤 채집 결과 목록이 없으면 스킵
        if (!HasGatherResults(sourceData))
        {
            if (!stack.IsEmpty) CheckStack(stack);
            return;
        }

        Vector3 sourcePos = stack.transform.position;

        // 날씨 2배 판정
        int count = sourceData.gatherResultCount;
        if (UnityEngine.Random.value < WeatherManager.GatherDoubleChance)
        {
            count *= 2;
            Debug.Log("[Weather] 채집 2배 발동!");
        }

        for (int i = 0; i < count; i++)
        {
            CardData result = PickGatherResult(sourceData);
            if (result != null)
                CardSpawner.Instance.SpawnNear(result, sourcePos, stack);
        }

        // SourceCard Gather 처리
        source.Gather();
        if (source.IsExhausted)
        {
            stack.cards.Remove(source);
            Destroy(source.gameObject);
            stack.Refresh();
        }

        if (stack != null && !stack.IsEmpty)
            CheckStack(stack);
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

    /// <summary>
    /// ingredients가 비어있는 특수 레시피(gatherRecipe, healRecipe)에서
    /// 대상 카드 목록이 해당 작업을 이어받을 수 있는지 판정.
    /// CardStack.SplitFrom()의 StageTransfer 여부 결정에 사용.
    /// </summary>
    public bool CanTransferTask(CardRecipe recipe, List<Card> cards)
    {
        if (recipe == null || cards == null) return false;
        if (recipe == gatherRecipe) return HasGatherPattern(cards);
        if (recipe == healRecipe)   return HasHealPattern(cards);
        return false;
    }

    private bool HasGatherPattern(List<Card> cards)
    {
        bool hasVillager = false, hasSource = false;
        foreach (var c in cards)
        {
            if (c is VillagerCard v)
            {
                if (v.data is VillagerCardData vd && !vd.isBaby) hasVillager = true;
            }
            else if (c is SourceCard)
            {
                hasSource = true;
            }
        }
        return hasVillager && hasSource;
    }

    private bool HasHealPattern(List<Card> cards)
    {
        bool hasVillager = false, hasHeart = false;
        foreach (var c in cards)
        {
            if (c is VillagerCard) hasVillager = true;
            if (c is HeartCard)    hasHeart = true;
        }
        return hasVillager && hasHeart;
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
            // cardResult / packResult 중 하나라도 채워져 있으면 SpawnResults 호출. 둘 다 채워졌으면 둘 다 스폰.
            if (recipe.cardResult != null || recipe.packResult != null)
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

        // 카드 결과 — 채워져 있으면 resultCount만큼 스폰 (날씨 더블 보너스 적용)
        if (recipe.cardResult != null)
        {
            int count = recipe.resultCount;
            // 자원 채집(스택에 SourceCard 포함) + 날씨 더블 확률 발동 시 2배
            if (StackHasSource(stack) &&
                UnityEngine.Random.value < WeatherManager.GatherDoubleChance)
            {
                count *= 2;
                Debug.Log("[Weather] 채집 2배 발동!");
            }

            for (int i = 0; i < count; i++)
                CardSpawner.Instance.SpawnNear(recipe.cardResult, sourcePos, stack);
        }

        // 카드팩 결과 — 채워져 있으면 1개 스폰 (resultCount 무관, 날씨 보너스 무관)
        if (recipe.packResult != null)
        {
            CardSpawner.Instance.SpawnPack(recipe.packResult, sourcePos);
        }
    }

    /// <summary>
    /// gatherResults 목록에서 가중치 기반으로 하나를 선택.
    /// EnemyManager.PickEnemy()와 동일한 누적 가중치 알고리즘.
    /// </summary>
    private CardData PickGatherResult(SourceCardData data)
    {
        if (data == null || data.gatherResults == null || data.gatherResults.Count == 0)
            return null;

        int total = 0;
        foreach (var e in data.gatherResults)
            if (e.data != null) total += Mathf.Max(0, e.weight);

        if (total <= 0)
            return null;

        int r = UnityEngine.Random.Range(0, total);
        int cumulative = 0;
        CardData lastValidResult = null;

        foreach (var e in data.gatherResults)
        {
            if (e.data == null) continue;

            lastValidResult = e.data;
            cumulative += Mathf.Max(0, e.weight);
            if (r < cumulative) return e.data;
        }

        return lastValidResult;
    }

    private bool HasGatherResults(SourceCardData data)
    {
        if (data == null || data.gatherResults == null)
            return false;

        foreach (var e in data.gatherResults)
        {
            if (e.data != null && e.weight > 0)
                return true;
        }

        return false;
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
            else if (recipe.consumeIngredients
                     && (recipe.preserveIngredients == null || !recipe.preserveIngredients.Contains(found.data)))
            {
                stack.cards.Remove(found);
                Destroy(found.gameObject);
            }
        }

        stack.Refresh();
    }
}
