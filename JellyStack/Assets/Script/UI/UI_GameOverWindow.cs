using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOverWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    private UIManager uiManager;

    public void Init(UIManager manager)
    {
        uiManager = manager;

        if (button != null) button.onClick.AddListener(uiManager.ExitGame);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


}
