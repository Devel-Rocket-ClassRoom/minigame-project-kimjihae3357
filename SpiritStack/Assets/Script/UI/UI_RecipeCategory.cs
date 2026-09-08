using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RecipeCategory : MonoBehaviour
{
    [Header("토글")]
    [SerializeField] private Button titleButton;
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private bool startOpen;

    [Header("목록 생성")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private UI_RecipeEntry entryPrefab;
    [SerializeField] private CardType resultType = CardType.None;
    [SerializeField] private List<CardRecipe> extraRecipes = new List<CardRecipe>();

    private readonly List<UI_RecipeEntry> spawnedEntries = new List<UI_RecipeEntry>();
    private UI_RecipeBook owner;
    private bool initialized;

    public void Init(UI_RecipeBook recipeBook)
    {
        owner = recipeBook;
        initialized = true;

        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(Toggle);
            titleButton.onClick.AddListener(Toggle);
        }

        SetOpen(startOpen);
    }

    private void Start()
    {
        if (!initialized)
            Init(null);
    }

    private void OnDestroy()
    {
        if (titleButton != null)
            titleButton.onClick.RemoveListener(Toggle);
    }

    public void Toggle()
    {
        bool nextOpen = !IsOpen;
        SetOpen(nextOpen);

        if (nextOpen && owner != null)
            owner.NotifyCategoryOpened(this);
    }

    public void SetOpen(bool isOpen)
    {
        GameObject panel = GetContentPanel();
        if (panel == null) return;

        panel.SetActive(isOpen);

        if (isOpen)
            RebuildLayout();
    }

    public void Build(IEnumerable<CardRecipe> recipes)
    {
        ClearEntries();

        if (contentRoot == null || entryPrefab == null || recipes == null)
            return;

        foreach (var recipe in recipes)
        {
            if (!ShouldShow(recipe))
                continue;

            UI_RecipeEntry entry = Instantiate(entryPrefab, contentRoot);
            entry.gameObject.SetActive(true);
            entry.SetRecipe(recipe);
            spawnedEntries.Add(entry);
        }

        RebuildLayout();
    }

    private bool IsOpen
    {
        get
        {
            GameObject panel = GetContentPanel();
            return panel != null && panel.activeSelf;
        }
    }

    private GameObject GetContentPanel()
    {
        if (contentPanel != null)
            return contentPanel;

        return contentRoot != null ? contentRoot.gameObject : null;
    }

    private bool ShouldShow(CardRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (extraRecipes.Contains(recipe))
            return true;

        if (resultType == CardType.None)
            return true;

        // cardResult가 있으면 그 cardType으로 필터링.
        // 카드 없이 packResult만 있는 레시피는 cardType이 없으므로 None(전체)에서만 노출.
        return recipe.cardResult != null && recipe.cardResult.cardType == resultType;
    }

    private void ClearEntries()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }

        spawnedEntries.Clear();
    }

    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();

        Rebuild(contentRoot);
        Rebuild(contentPanel != null ? contentPanel.transform as RectTransform : null);
        Rebuild(transform as RectTransform);
        Rebuild(transform.parent as RectTransform);

        Canvas.ForceUpdateCanvases();
    }

    private void Rebuild(RectTransform rectTransform)
    {
        if (rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
