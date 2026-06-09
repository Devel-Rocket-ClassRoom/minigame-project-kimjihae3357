using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UI_GameOverWindow gameover;
    [SerializeField] private UI_PauseWindow pause;
    [Tooltip("새 게임 시작 시 자동으로 띄울 튜토리얼 창. 이어하기일 땐 띄우지 않음.")]
    [SerializeField] private UI_TutorialWindow tutorial;

    private bool isPaused;

    private void Start()
    {
        gameover.Init(this);
        gameover.Hide();
        pause.Init(this);
        pause.Hide();

        // 새 게임이면 튜토리얼을 먼저 띄우고 게임 시간을 정지.
        // Close 버튼이 눌리면 OnClosed 콜백에서 timeScale을 1로 복원해 진행 시작.
        // 이어하기(LoadRequested=true)일 땐 SaveManager.RestoreRoutine이 timeScale=1을 직접 보장하므로 충돌 없음.
        if (tutorial != null)
        {
            if (!SaveSystem.LoadRequested)
            {
                tutorial.OnClosed += HandleTutorialClosed;
                Time.timeScale = 0f;
                tutorial.Show();
            }
            else
            {
                tutorial.Hide();
            }
        }
    }

    private void HandleTutorialClosed()
    {
        if (tutorial != null) tutorial.OnClosed -= HandleTutorialClosed;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (tutorial != null) tutorial.OnClosed -= HandleTutorialClosed;
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

    public void SaveGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SaveSystem.LoadRequested = false;   // 재시작은 새 게임 취급

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Title");
    }
}
