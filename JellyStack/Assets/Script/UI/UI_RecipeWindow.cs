using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UI_Recipe : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject panel;

    private void Start()
    {
        panel.gameObject.SetActive(false);
        button.onClick.AddListener(TogglePanel);
    }

    private void TogglePanel()
    {
        bool isOpen = !panel.activeSelf;
        panel.SetActive(isOpen);
    }


}
