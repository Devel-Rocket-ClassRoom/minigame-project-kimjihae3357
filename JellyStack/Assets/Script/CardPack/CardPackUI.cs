using TMPro;
using UnityEngine;

public class CardPackUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private CardPackData cardPack;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cardPack == null)
            return;

        if (spriteIcon != null) spriteIcon.sprite = cardPack.Image;
        if (nameText != null) nameText.text = cardPack.cardName;

    }
}
