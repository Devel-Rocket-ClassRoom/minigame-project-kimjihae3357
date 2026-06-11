using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 엔딩 창. 보스 처치 시 GameManager.ShowGameEnding()을 통해 표시되며,
/// "게임으로 돌아가기" 버튼을 누르면 OnClosed 이벤트를 발화시켜 UIManager가 timeScale=1로 복원한다.
/// </summary>
public class UI_GameEndingWindow : MonoBehaviour
{
    [Tooltip("\"게임으로 돌아가기\" 버튼. 누르면 창이 닫히고 게임이 다시 진행됨.")]
    [SerializeField] private Button returnButton;

    /// <summary>창이 닫힐 때 발화. UIManager가 구독해 timeScale 복원에 사용.</summary>
    public System.Action OnClosed;

    private void Awake()
    {
        if (returnButton != null) returnButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }
}
