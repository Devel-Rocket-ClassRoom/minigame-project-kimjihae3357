using UnityEngine;

[CreateAssetMenu(fileName = "FoodCardData", menuName = "Card/FoodCardData")]
public class FoodCardData : CardData
{
    [Header("음식 카드")]
    public int fullnessValue;
}
