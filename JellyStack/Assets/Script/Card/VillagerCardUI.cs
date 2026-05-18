using TMPro;
using UnityEngine;

public class VillagerCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text hungerText;

    private VillagerCard villager;

    private void Awake()
    {
        villager = GetComponent<VillagerCard>();
    }

    private void OnEnable()
    {
        villager.OnStatChanged += UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        spriteIcon.sprite = villager.data.icon;
        nameText.text = villager.data.cardName;
        healthText.text = villager.CurrentHealth.ToString();
        hungerText.text = villager.Currenthunger.ToString();
    }
}
