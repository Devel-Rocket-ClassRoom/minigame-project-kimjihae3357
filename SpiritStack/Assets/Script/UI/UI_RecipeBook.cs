using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RecipeBook : MonoBehaviour
{
    [Header("전체 패널 토글")]
    [SerializeField] private Button bookButton;
    [SerializeField] private GameObject bookPanel;
    [SerializeField] private bool startOpen;
    [SerializeField] private bool closeCategoriesWhenBookCloses = true;

    [Header("카테고리")]
    [SerializeField] private List<UI_RecipeCategory> categories = new List<UI_RecipeCategory>();
    [SerializeField] private bool closeOtherCategoriesOnOpen;

    [Header("레시피 데이터")]
    [SerializeField] private bool useRecipeManager = true;
    [SerializeField] private List<CardRecipe> fallbackRecipes = new List<CardRecipe>();

    private void Awake()
    {
        if (categories.Count == 0)
            categories.AddRange(GetComponentsInChildren<UI_RecipeCategory>(true));

        if (bookButton != null)
            bookButton.onClick.AddListener(ToggleBook);

        foreach (var category in categories)
        {
            if (category != null)
                category.Init(this);
        }
    }

    private void Start()
    {
        Rebuild();
        SetBookOpen(startOpen);
    }

    private void OnDestroy()
    {
        if (bookButton != null)
            bookButton.onClick.RemoveListener(ToggleBook);
    }

    public void ToggleBook()
    {
        SetBookOpen(!IsBookOpen);
    }

    public void SetBookOpen(bool isOpen)
    {
        if (bookPanel == null) return;

        bookPanel.SetActive(isOpen);
        if (!isOpen && closeCategoriesWhenBookCloses)
        {
            foreach (var category in categories)
            {
                if (category != null)
                    category.SetOpen(false);
            }
        }
    }

    public void Rebuild()
    {
        IEnumerable<CardRecipe> recipes = GetRecipes();
        foreach (var category in categories)
        {
            if (category != null)
                category.Build(recipes);
        }
    }

    public void NotifyCategoryOpened(UI_RecipeCategory openedCategory)
    {
        if (!closeOtherCategoriesOnOpen) return;

        foreach (var category in categories)
        {
            if (category != null && category != openedCategory)
                category.SetOpen(false);
        }
    }

    private bool IsBookOpen => bookPanel != null && bookPanel.activeSelf;

    private IEnumerable<CardRecipe> GetRecipes()
    {
        if (useRecipeManager && RecipeManager.Instance != null)
            return RecipeManager.Instance.Recipes;

        return fallbackRecipes;
    }
}
