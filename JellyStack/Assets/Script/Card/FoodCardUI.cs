using UnityEngine;
using TMPro;

public class FoodCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text fullnessText;

    private FoodCard food;

    private void Awake()
    {
        food = GetComponent<FoodCard>();
    }

    private void OnEnable()
    {
        if (food != null)
            food.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (food != null)
            food.OnStatChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (food == null || food.data == null)
            return;

        if (spriteIcon != null) spriteIcon.sprite = food.data.Image;
        if (nameText != null) nameText.text = food.data.cardName;
        if (fullnessText != null) fullnessText.text = food.CurrentFullness.ToString();
    }
}
