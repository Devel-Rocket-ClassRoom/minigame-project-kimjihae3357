using UnityEngine;
using UnityEngine.UI;

public class UI_CardProgressBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject visualRoot;

    // Hide()를 Awake에서 호출. Start로 두면 Instantiate 직후 같은 프레임에 외부에서
    // Show()를 호출해도, 다음 프레임 Start()의 Hide()가 다시 끄는 타이밍 버그가 생긴다.
    // Awake는 Instantiate 직후 즉시 호출되므로 그 이후의 Show() 호출이 안전하게 유지된다.
    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        if (visualRoot != null)
            visualRoot.SetActive(true);
    }

    public void Hide()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);
    }

    public void SetProgress(float value)
    {
        if (slider != null)
            slider.value = Mathf.Clamp01(value);
    }
}
