using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Ingame : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text nextDayText;
    [SerializeField] private CanvasGroup dayChangeGroup;

    private void Start()
    {
        DayManager.Instance.OnDayChanged += HandleDayChanged;
        dayText.text = $"{DayManager.Instance.CurrentDay}";

        if (dayChangeGroup != null)
        {
            dayChangeGroup.alpha = 0f;
            dayChangeGroup.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
    }

    private void Update()
    {
        int minutes = (int)(DayManager.Instance.ElapsedTime / 60f);
        int seconds = (int)(DayManager.Instance.ElapsedTime % 60f);
        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void HandleDayChanged(int newDay)
    {
        dayText.text = $"{newDay}";
        nextDayText.text = $"{newDay}";
        Time.timeScale = 0f;
        if (dayChangeGroup != null)
            StartCoroutine(DayChangeEffect());
    }

    private IEnumerator DayChangeEffect()
    {
        dayChangeGroup.gameObject.SetActive(true);
        dayChangeGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(1.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            dayChangeGroup.alpha = 1f - t;
            yield return null;
        }
        dayChangeGroup.alpha = 0f;
        Time.timeScale = 1f;
        dayChangeGroup.gameObject.SetActive(false);
    }
}
