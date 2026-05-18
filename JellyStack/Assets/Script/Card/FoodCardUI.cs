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
        food.OnStatChanged += UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        spriteIcon.sprite = food.data.icon;
        nameText.text = food.data.cardName;
        fullnessText.text = food.CurrentFullness.ToString();
    }
}
