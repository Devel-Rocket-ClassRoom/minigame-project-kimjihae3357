using TMPro;
using UnityEngine;

public class ResourceCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;

    private Card card;
    private void Start()
    {
        card = GetComponent<Card>();
        UpdateUI();
    }

    private void UpdateUI()
    {
        spriteIcon.sprite = card.data.icon;
        nameText.text = card.data.cardName;
    }
}
