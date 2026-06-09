using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_RecipeEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text recipeText;
    [SerializeField] private string ingredientSeparator = "+";

    [Header("색상")]
    [SerializeField] private Color resultColor = new Color(0.42f, 0.23f, 0.12f);
    [SerializeField] private Color ingredientsColor = new Color(0.69f, 0.42f, 0.29f);

    public void SetRecipe(CardRecipe recipe)
    {
        if (recipe == null) return;

        if (recipeText != null)
        {
            recipeText.richText = true;
            recipeText.text =
                $"<color=#{ColorUtility.ToHtmlStringRGB(resultColor)}>{GetResultText(recipe)}</color>: " +
                $"<color=#{ColorUtility.ToHtmlStringRGB(ingredientsColor)}>{GetIngredientsText(recipe.ingredients)}</color>";
        }
    }

    private string GetResultText(CardRecipe recipe)
    {
        // 자동 분기: cardResult / packResult 둘 다 있으면 둘 다 표시 (" + "로 연결).
        var parts = new List<string>();

        if (recipe.cardResult != null)
        {
            string cardName = GetCardName(recipe.cardResult, recipe.name);
            parts.Add(recipe.resultCount > 1 ? $"{cardName} x{recipe.resultCount}" : cardName);
        }

        if (recipe.packResult != null)
        {
            string packName = !string.IsNullOrEmpty(recipe.packResult.cardName)
                ? recipe.packResult.cardName
                : recipe.packResult.name;
            parts.Add(packName);
        }

        return parts.Count > 0 ? string.Join(" + ", parts) : recipe.name;
    }

    private string GetIngredientsText(List<CardData> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0)
            return "-";

        var counts = new Dictionary<string, int>();
        var order = new List<string>();

        foreach (var ingredient in ingredients)
        {
            string ingredientName = GetCardName(ingredient, "-");

            if (!counts.ContainsKey(ingredientName))
            {
                counts[ingredientName] = 0;
                order.Add(ingredientName);
            }

            counts[ingredientName]++;
        }

        var builder = new StringBuilder();
        for (int i = 0; i < order.Count; i++)
        {
            if (i > 0)
                builder.Append(ingredientSeparator);

            string ingredientName = order[i];
            int count = counts[ingredientName];

            builder.Append(count > 1 ? $"{ingredientName}{count}" : ingredientName);
        }

        return builder.ToString();
    }

    private string GetCardName(CardData cardData, string fallback)
    {
        if (cardData == null)
            return fallback;

        if (!string.IsNullOrEmpty(cardData.cardName))
            return cardData.cardName;

        return cardData.name;
    }
}
