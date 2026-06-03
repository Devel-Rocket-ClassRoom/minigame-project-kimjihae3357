using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_TitleWindow : MonoBehaviour
{

    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        startButton.onClick.AddListener(NewGame);
        continueButton.onClick.AddListener(ContinueGame);
        exitButton.onClick.AddListener(ExitGame);

    }


    public void NewGame()
    {
        SceneManager.LoadScene("Ingame");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Ingame");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
