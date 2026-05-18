using TMPro;
using UnityEngine;

public class SourceCardUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    private SourceCard source;

    private void Awake()
    {
        source = GetComponent<SourceCard>();
    }

    private void OnEnable()
    {
        source.OnStatChanged += UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        spriteIcon.sprite = source.data.icon;
        nameText.text = source.data.cardName;
        countText.text = source.CurrentCount.ToString();

    }
}
