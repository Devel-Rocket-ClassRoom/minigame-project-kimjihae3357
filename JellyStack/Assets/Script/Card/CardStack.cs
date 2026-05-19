using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    [Header("UI")]
    [SerializeField] private CardProgressBarUI progressBar;
    public CardProgressBarUI ProgressBar => progressBar;

    public Card TopCard => cards.Count > 0 ? cards[cards.Count - 1] : null;
    public Card BottomCard => cards.Count > 0 ? cards[0] : null;
    public bool IsEmpty => cards.Count == 0;

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
            task.Cancel();

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

    private void UpdateProgressBarPosition()
    {
        if (progressBar == null || cards.Count == 0)
            return;

        int topIndex = cards.Count - 1;
        Vector3 topCardLocalPos = new Vector3(0, 0.01f * topIndex, -0.7f * topIndex);

        progressBar.transform.localPosition = topCardLocalPos + new Vector3(0, 0.5f, 0.3f);
    }
}
