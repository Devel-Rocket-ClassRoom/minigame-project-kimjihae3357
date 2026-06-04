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
        if (source != null)
            source.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (source != null)
            source.OnStatChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (source == null || source.data == null)
            return;

        if (spriteIcon != null) spriteIcon.sprite = source.data.Image;
        if (nameText != null) nameText.text = source.data.cardName;
        if (countText != null) countText.text = source.CurrentCount.ToString();

    }
}
