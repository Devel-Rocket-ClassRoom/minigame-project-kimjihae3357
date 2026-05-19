using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardRecipe", menuName = "Card/CardRecipe")]
public class RecipeManager : ScriptableObject
{
    [Header("재료")]
    public List<CardData> ingredients = new List<CardData>();

    [Header("결과물")]
    public CardData result;
    public int resultCount = 1;

    [Header("작업")]
    public float duration = 3;
    public bool consumeIngredients = true;

    [Header("표시 아이콘")]
    public GameObject iconPrefab;

    private void SpawnResult(CardData data, Vector3 nearPos)
    {
        Vector3 spawnPos = nearPos + new Vector3(2f, 0, 0);
        CardSpawner.Instance.Spawn(data, spawnPos);
    }
}
