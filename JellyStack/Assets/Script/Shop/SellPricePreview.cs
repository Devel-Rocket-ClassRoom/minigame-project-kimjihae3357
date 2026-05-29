using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SellPoint 위에 표시되는 판매 가격 미리보기 UI.
/// 드래그 중인 스택이 SellPoint 영역에 들어오면 Show(price), 벗어나면 Hide().
/// </summary>
public class SellPricePreview : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private SpriteRenderer coinIcon;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(int price)
    {
        if (priceText != null) priceText.text = price.ToString();
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
