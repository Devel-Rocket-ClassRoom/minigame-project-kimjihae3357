using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardRecipe", menuName = "Card/CardRecipe")]
public class CardRecipe : ScriptableObject
{
    [Header("재료")]
    public List<CardData> ingredients = new List<CardData>();

    [Header("결과물")]
    public CardData result;
    public int resultCount = 1;

    [Header("작업")]
    public float duration = 3;
    [Header("카드 소비됨")]
    public bool consumeIngredients = true;

    [Header("표시 아이콘")]
    public GameObject iconPrefab;

}
