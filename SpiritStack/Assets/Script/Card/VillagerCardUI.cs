using TMPro;
using UnityEngine;

public class VillagerCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text hungerText;
    [SerializeField] private TMP_Text attackText;

    private VillagerCard villager;
    private VillagerCardData villagerData;

    private void Awake()
    {
        villager = GetComponent<VillagerCard>();
        villagerData = villager != null ? villager.data as VillagerCardData : null;
    }

    private void OnEnable()
    {
        if (villager != null)
            villager.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (villager != null)
            villager.OnStatChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (villager == null || villager.data == null)
            return;

        villagerData = villager.data as VillagerCardData;

        if (spriteIcon != null) spriteIcon.sprite = villager.data.Image;
        if (nameText != null) nameText.text = villager.data.cardName;
        if (healthText != null) healthText.text = villager.CurrentHealth.ToString();
        if (hungerText != null) hungerText.text = villager.Currenthunger.ToString();
        if (attackText != null && villagerData != null)
            attackText.text = villagerData.attackPower.ToString();
    }
}
