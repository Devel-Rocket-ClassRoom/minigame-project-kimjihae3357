using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_LoginWindow : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("버튼")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button openSignupButton;

    [Header("연결")]
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private GameObject signupWindow;

    private DOTweenAnimation anim;

    private void Awake()
    {
        anim = GetComponent<DOTweenAnimation>();
    }

    private void OnEnable()
    {
        if (anim != null)
            anim.DORestart();
    }

    private void Start()
    {
        ShowWarning("");
        loginButton.onClick.AddListener(OnLoginClicked);
        openSignupButton.onClick.AddListener(OpenSignup);
    }



    private async void OnLoginClicked()
    {
        string email = emailInput.text;
        string pw = passwordInput.text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pw))
        {
            ShowWarning("이메일과 비밀번호를 입력하세요.");
            return;
        }

        loginButton.interactable = false;
        ShowWarning("로그인 중...");

        var (ok, error) = await FirebaseManager.Instance.SignInAsync(email, pw);

        loginButton.interactable = true;

        if (ok)
        {
            if (UI_TitleWindow.Instance != null)
                await UI_TitleWindow.Instance.OnSignedIn();
            else
                Hide();
        }
        else ShowWarning(error);
    }

    private void OpenSignup()
    {
        if (warningText != null) warningText.text = "";
        if (signupWindow != null) signupWindow.SetActive(true);
    }

    private void ShowWarning(string msg)
    {
        if (warningText != null) warningText.text = msg;
    }

    public void Hide() => gameObject.SetActive(false);
}
