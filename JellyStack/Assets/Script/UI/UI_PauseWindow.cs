using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PauseWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    private UIManager uiManager;

    public void Init(UIManager manager)
    {
        uiManager = manager;

        resumeButton.onClick.AddListener(uiManager.ResumeGame);
        exitButton.onClick.AddListener(uiManager.ExitGame);
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
