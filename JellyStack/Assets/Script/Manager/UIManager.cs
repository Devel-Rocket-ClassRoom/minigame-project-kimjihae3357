using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UI_GameOverWindow gameover;
    [SerializeField] private UI_PauseWindow pause;

    private bool isPaused;

    private void Start()
    {
        gameover.Hide();
        pause.Init(this);
        pause.Hide();
    }

    public void TogglePause()
    {
        if (gameover != null && gameover.gameObject.activeSelf) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        pause.Show();

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pause.Hide();
        gameover.Hide();

        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        gameover.Show();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Title");
    }
}
