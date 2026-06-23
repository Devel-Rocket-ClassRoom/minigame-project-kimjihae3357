using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_SignupWindow : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("버튼")]
    [SerializeField] private Button signupButton;
    [SerializeField] private Button backButton;

    [Header("연결")]
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private GameObject loginWindow;

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
        signupButton.onClick.AddListener(OnSignupClicked);
        backButton.onClick.AddListener(BackToLogin);
    }

    private async void OnSignupClicked()
    {
        string email = emailInput.text;
        string pw = passwordInput.text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pw))
        {
            ShowWarning("이메일과 비밀번호를 입력하세요");
            return;
        }

        signupButton.interactable = false;
        ShowWarning("가입 중...");

        var (ok, error) = await FirebaseManager.Instance.SignUpAsync(email, pw);

        signupButton.interactable = true;

        if (ok)
        {
            ShowWarning("");
            gameObject.SetActive(false);
            if (UI_TitleWindow.Instance != null)
                await UI_TitleWindow.Instance.OnSignedIn();
        }
        else ShowWarning(error);
    }

    private void BackToLogin()
    {
        ShowWarning("");
        gameObject.SetActive(false);
    }

    private void ShowWarning(string msg)
    {
        if (warningText != null) warningText.text = msg;
    }
}
