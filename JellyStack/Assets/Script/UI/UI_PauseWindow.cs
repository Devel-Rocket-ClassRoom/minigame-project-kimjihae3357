using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PauseWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button saveButton; 
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    private UIManager uiManager;

    public void Init(UIManager manager)
    {
        uiManager = manager;

        resumeButton.onClick.AddListener(uiManager.ResumeGame);
        exitButton.onClick.AddListener(uiManager.ExitGame);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        uiManager.SaveGame();
        if (text != null) text.text = "저장 완료";
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
