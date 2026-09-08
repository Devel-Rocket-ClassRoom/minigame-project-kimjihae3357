using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class UI_TitleWindow : MonoBehaviour
{
    public static UI_TitleWindow Instance { get; private set;  }

    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    [Header("세이브 정보")]
    [Tooltip("저장된 날짜를 'N일째'로 표시할 텍스트.")]
    [SerializeField] private TMP_Text saveDataText;
    [Tooltip("Continue 버튼의 부모 오브젝트. 세이브 없을 때 숨길 대상.")]
    [SerializeField] private GameObject continueContainer;

    [Header("로그인 창")]
    [SerializeField] private GameObject loginWindow;

    [Header("로그인 정보")]
    [SerializeField] private GameObject loginInfo;
    [SerializeField] private TMP_Text loginIdText;
    [SerializeField] private Button logoutButton;

    private void Awake() => Instance = this; 

    private async Task Start()
    {
        startButton.onClick.AddListener(NewGame);
        continueButton.onClick.AddListener(ContinueGame);
        exitButton.onClick.AddListener(ExitGame);

        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClicked);

        if (FirebaseManager.Instance != null)
            await FirebaseManager.Instance.ReadyTask;

        bool signedIn = FirebaseManager.Instance != null && FirebaseManager.Instance.IsSignedIn;
        if (signedIn)
            await OnSignedIn();
        else
        {
            if (loginWindow != null)
                loginWindow.SetActive(true);
            if (loginInfo != null)
                loginInfo.SetActive(false);
        }
    }

    public async Task OnSignedIn()
    {
        if (loginWindow != null)
            loginWindow.SetActive(false);

        if (loginInfo != null)
            loginInfo.SetActive(true);

        if (loginIdText != null)
            loginIdText.text = FirebaseManager.Instance.Email;

        var cloud = await FirebaseManager.Instance.LoadGameAsync();
        if (cloud != null)
            SaveSystem.Write(cloud);
        else SaveSystem.Delete();

        RefreshSaveInfo();
    }

    public void OnLogoutClicked()
    {
        FirebaseManager.Instance.SignOut();

        if (loginInfo != null)
            loginInfo.SetActive(false );

        if (loginWindow != null)
            loginWindow.SetActive(true );

        SaveSystem.Delete();
        RefreshSaveInfo() ;
    }

    private void RefreshSaveInfo()
    {
        bool hasSave = SaveSystem.HasSave();

        // continueContainer가 있으면 부모를 토글, 없으면 버튼 자체를 토글
        var target = continueContainer != null
            ? continueContainer
            : continueButton?.gameObject;
        if (target != null) target.SetActive(hasSave);

        if (saveDataText != null)
        {
            int day = 0;
            if (hasSave)
            {
                var data = SaveSystem.Read();
                if (data != null) day = data.currentDay;
            }
            saveDataText.text = $"{day:00}일째";
        }
    }

    public void NewGame()
    {
        SaveSystem.LoadRequested = false;   // 처음부터 시작
        SceneManager.LoadScene("Ingame");
    }

    public void ContinueGame()
    {
        if (!SaveSystem.HasSave()) return;  // 세이브 없으면 무시
        SaveSystem.LoadRequested = true;    // Ingame에서 복원
        SceneManager.LoadScene("Ingame");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
