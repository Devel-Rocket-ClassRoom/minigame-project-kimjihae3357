using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PauseWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;

    private UIManager uiManager;

    public void Init(UIManager manager)
    {
        uiManager = manager;

        button.onClick.AddListener(uiManager.ResumeGame);
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
