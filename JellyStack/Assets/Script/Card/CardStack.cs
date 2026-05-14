using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    public Card TopCard => cards.Count > 0 ? cards[cards.Count - 1] : null;
    public Card BottomCard => cards.Count > 0 ? cards[0] : null;
    public bool IsEmpty => cards.Count == 0;

    public void AddCard(Card card)
    {
        cards.Add(card);
        card.stack = this;
        card.transform.SetParent(transform);
        ArrangeCards();
    }

    public void AddCards(List<Card> newCards)
    {
        foreach (var c in newCards)
        {
            cards.Add(c);
            c.stack = this;
            c.transform.SetParent(transform);
        }
        ArrangeCards();
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
        }

        ArrangeCards();
        return moved;
    }

    private void ArrangeCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition =
                new Vector3(0, 0.01f * i, -0.25f * i);


        }
    }
}
