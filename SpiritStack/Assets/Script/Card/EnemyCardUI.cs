using TMPro;
using UnityEngine;

public class EnemyCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;

    private EnemyCard enemy;
    private EnemyCardData enemyData;

    private void Awake()
    {
        enemy = GetComponent<EnemyCard>();
        enemyData = enemy != null ? enemy.data as EnemyCardData : null;
    }

    private void OnEnable()
    {
        if (enemy != null)
            enemy.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.OnStatChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemy == null || enemy.data == null)
            return;

        enemyData = enemy.data as EnemyCardData;

        if (spriteIcon != null) spriteIcon.sprite = enemy.data.Image;
        if (nameText != null) nameText.text = enemy.data.cardName;
        if (healthText != null) healthText.text = enemy.CurrentHealth.ToString();
        if (attackText != null && enemyData != null)
            attackText.text = enemyData.attackPower.ToString();
    }
}
