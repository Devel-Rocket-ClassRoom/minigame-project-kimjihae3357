using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{

    public static RecipeManager Instance { get; private set; }

    [Header("등록된 레시피 목록")]
    [SerializeField] private List<CardRecipe> recipes = new List<CardRecipe>();

    [Header("결과물 스폰 위치 오프셋")]
    [SerializeField] private Vector3 resultSpawnOffset = new Vector3(2f, 0, 0);
    [SerializeField] private float spawnRadiusMin = 1.5f;
    [SerializeField] private float spawnRadiusMax = 2.5f;
    [SerializeField] private float occupiedRadius = 1.4f;
    [SerializeField] private float autoStackRadius = 7f; //해당 반경안에서 자동 스택
    [SerializeField] private int randomSpawnAttempts = 12;
    [SerializeField] private int searchRingCount = 4;
    [SerializeField] private int searchSlotsPerRing = 12;

    [SerializeField] private LayerMask cardMask;
    [SerializeField] private Vector3 spawnCheckHalfExtents = new Vector3(0.8f, 0.2f, 1.1f);

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

        // 1. 결과물 스폰 (consumeIngredients와 무관)
        if (recipe.result != null)
            SpawnResults(recipe, stack);

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
        if (CardSpawner.Instance == null) return;

        Vector3 sourcePos = stack.transform.position;

        for (int i = 0; i < recipe.resultCount; i++)
        {
            CardStack sameCardStack = FindSameCardStackNearby(recipe.result, sourcePos, stack);
            Card card;

            if (sameCardStack != null)
            {
                card = CardSpawner.Instance.SpawnIntoStack(recipe.result, sameCardStack);
            }
            else
            {
                Vector3 spawnPos = FindEmptySpawnPosition(sourcePos);
                card = CardSpawner.Instance.Spawn(recipe.result, spawnPos);
            }

            if (card != null)
                card.transform.position = sourcePos;
        }
    }

    private CardStack FindSameCardStackNearby(CardData data, Vector3 center, CardStack exclude)
    {
        if (data == null)
            return null;

        CardStack[] allStacks = Object.FindObjectsByType<CardStack>(FindObjectsSortMode.None);
        CardStack nearestStack = null;
        float nearestDistance = autoStackRadius;

        foreach (var stack in allStacks)
        {
            if (stack == null || stack == exclude || stack.IsEmpty)
                continue;

            if (!IsSameCardOnlyStack(stack, data))
                continue;

            float distance = GetClosestStackDistance(center, stack);
            if (distance < nearestDistance)
            {
                nearestStack = stack;
                nearestDistance = distance;
            }
        }

        return nearestStack;
    }

    private bool IsSameCardOnlyStack(CardStack stack, CardData data)
    {
        foreach (var card in stack.cards)
        {
            if (card == null || card.data != data)
                return false;
        }

        return true;
    }

    private float GetClosestStackDistance(Vector3 center, CardStack stack)
    {
        Vector2 centerPos = new Vector2(center.x, center.z);
        float closestDistance = Vector2.Distance(
            centerPos,
            new Vector2(stack.transform.position.x, stack.transform.position.z)
        );

        foreach (var card in stack.cards)
        {
            if (card == null)
                continue;

            Vector2 cardPos = new Vector2(card.transform.position.x, card.transform.position.z);
            closestDistance = Mathf.Min(closestDistance, Vector2.Distance(centerPos, cardPos));
        }

        return closestDistance;
    }

    private Vector3 FindEmptySpawnPosition(Vector3 center)
    {
        Vector3 preferredPos = center + resultSpawnOffset;
        if (IsSpawnPositionEmpty(preferredPos))
            return preferredPos;

        for (int i = 0; i < randomSpawnAttempts; i++)
        {
            Vector3 candidate = GetRandomPositionAround(center);
            if (IsSpawnPositionEmpty(candidate))
                return candidate;
        }

        for (int ring = 0; ring < searchRingCount; ring++)
        {
            float distance = spawnRadiusMin + occupiedRadius * ring;
            int slotCount = searchSlotsPerRing + ring * 4;
            float angleOffset = Random.Range(0f, 360f / slotCount);

            for (int slot = 0; slot < slotCount; slot++)
            {
                float angle = (angleOffset + slot * 360f / slotCount) * Mathf.Deg2Rad;
                Vector3 candidate = center + new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );

                if (IsSpawnPositionEmpty(candidate))
                    return candidate;
            }
        }

        return GetRandomPositionAround(center);
    }

    private bool IsSpawnPositionEmpty(Vector3 position)
    {
        // 근처에 빈자리 체크
        Collider[] hits = Physics.OverlapBox(
            position,
            spawnCheckHalfExtents,
            Quaternion.identity,
            cardMask
            );

        return hits.Length == 0;
    }

    private Vector3 GetRandomPositionAround(Vector3 center)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);

        return center + new Vector3(
            Mathf.Cos(angle) * distance,
            0,
            Mathf.Sin(angle) * distance
        );
    }
}
