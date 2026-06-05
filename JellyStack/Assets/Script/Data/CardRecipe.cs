using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardRecipe", menuName = "CardRecipe/CardRecipe")]
public class CardRecipe : ScriptableObject
{
    [Header("재료")]
    public List<CardData> ingredients = new List<CardData>();

    [Header("유지할 재료")]
    [Tooltip("consumeIngredients=true 이더라도 이 목록의 재료는 삭제하지 않고 스택에 남긴다.")]
    public List<CardData> preserveIngredients = new List<CardData>();

    [Header("결과물")]
    public CardData result;
    public int resultCount = 1;

    [Header("작업 시간")]
    public float duration = 3;
    [Header("카드 소비됨")]
    public bool consumeIngredients = true;

    [Header("표시 아이콘")]
    public GameObject iconPrefab;

    [Header("레시피 이펙트")]
    public GameObject effectPrefab;   // 진행 중 카드 위에 표시할 이펙트 (선택)
}
