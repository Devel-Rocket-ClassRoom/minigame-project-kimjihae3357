using UnityEngine;
using UnityEngine.UI;

public class CardProgressBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject visualRoot;

    private void Start()
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
