using TMPro;
using UnityEngine;

public class CoinCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;

    private Card card;
    private void Awake()
    {
        card = GetComponent<Card>();
        if (card == null) card = GetComponentInParent<Card>();
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (card == null || card.data == null) return;

        if (spriteIcon != null) spriteIcon.sprite = card.data.Image;
        if (nameText != null) nameText.text = card.data.cardName;
    }
}
