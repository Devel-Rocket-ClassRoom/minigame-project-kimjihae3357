using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    [Header("UI")]
    [SerializeField] private UI_CardProgressBar progressBar;
    public UI_CardProgressBar ProgressBar => progressBar;

    [SerializeField] private LayerMask cardMask;
    [SerializeField] private float pushRadius = 2;
    [SerializeField] private float pushStrength = 5f; //카드 겹칠때 밀어내는 범위 및 세기

    public bool IsDragging { get; set; }

    public Card TopCard => cards.Count > 0 ? cards[cards.Count - 1] : null;
    public Card BottomCard => cards.Count > 0 ? cards[0] : null;
    public bool IsEmpty => cards.Count == 0;

    private void LateUpdate()
    {
        ResolveOverlap();
    }

    private void ResolveOverlap()
    {
        if (IsDragging || IsEmpty)
            return;

        // 전투 영역(BattlePoint)은 밀어내기 시스템에서 제외 (자기 자신이 BattlePoint인 경우)
        if (this is BattlePoint) return;

        // 근처에 겹치는걸 체크
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            pushRadius,
            cardMask
            );

        foreach (var hit in hits)
        {
            Card otherCard = hit.GetComponent<Card>();
            if (otherCard == null || otherCard.stack == null)
                continue;

            CardStack otherStack = otherCard.stack;

            if (otherStack == this || otherStack.IsEmpty || otherStack.IsDragging)
                continue;

            // BattlePoint 카드는 밀어내기 대상에서 제외
            if (otherStack is BattlePoint)
                continue;

            Vector3 direction = transform.position - otherStack.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                direction = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));

            transform.position += direction.normalized * pushStrength * Time.deltaTime;
        }
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
        card.stack = this;
        card.transform.SetParent(transform);
        if (card.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        ArrangeCards();
    }

    public void AddCards(List<Card> newCards, bool snapToOrigin = false)
    {
        foreach (var c in newCards)
        {
            cards.Add(c);
            c.stack = this;
            c.transform.SetParent(transform);
            if (c.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
            if (snapToOrigin) c.transform.localPosition = Vector3.zero;
        }
        ArrangeCards();

        if (RecipeManager.Instance != null)
            RecipeManager.Instance.CheckStack(this);
    }

    public List<Card> SplitFrom(Card card)
    {
        // card부터 위에 쌓인 카드 전부를 떼어내서 반환
        int idx = cards.IndexOf(card);
        if (idx < 0) return null;

        int count = cards.Count - idx;
        var moved = cards.GetRange(idx, count);
        cards.RemoveRange(idx, count);

        foreach (var c in moved)
        {
            c.stack = null;
            c.transform.SetParent(null);
            if (c.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;
        }

        var task = GetComponent<ProgressTask>();
        if (task != null)
        {
            var activeRecipe = task.Recipe;
            bool stillValid =
                activeRecipe != null &&
                cards.Count >= 2 &&
                RecipeManager.Instance != null &&
                RecipeManager.Instance.StackMatchesIngredients(this, activeRecipe.ingredients);

            if (!stillValid)
            {
                bool movedCarriesRecipe =
                    activeRecipe != null &&
                    moved.Count >= 2 &&
                    RecipeManager.Instance != null &&
                    RecipeManager.Instance.CardsMatchIngredients(moved, activeRecipe.ingredients);

                if (movedCarriesRecipe)
                    RecipeManager.Instance.StageTransfer(activeRecipe, task.Elapsed);

                task.Cancel();
            }
        }

        ArrangeCards();
        return moved;
    }

    // 카드가 찰딱 붙을 때
    public void Refresh() => ArrangeCards();

    private void ArrangeCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].targetLocalPosition = new Vector3(0, 0.01f * i, -0.7f * i);
            cards[i].followSpeed = Mathf.Max(20f, 20f - i * 3f);
        }
    }

}
