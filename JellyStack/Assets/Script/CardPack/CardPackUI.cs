using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardPackUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private CardPackData cardPack;

    private BuyPoint point;

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>BuyPoint에서 스폰 직후 데이터를 주입. Start() 전에 호출해야 함.</summary>
    public void SetData(CardPackData data, BuyPoint buyPoint)
    {
        cardPack = data;
        point    = buyPoint;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cardPack == null) return;

        if (spriteIcon != null) spriteIcon.sprite = cardPack.Image;
        if (nameText != null)   nameText.text     = cardPack.cardName;
        if (priceText != null && point != null) priceText.text = point.Price.ToString();
    }
}
