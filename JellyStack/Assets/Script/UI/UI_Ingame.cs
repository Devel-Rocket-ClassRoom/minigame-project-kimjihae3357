using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Ingame : MonoBehaviour
{
    public static UI_Ingame Instance { get; private set; }

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text nextDayText;
    [SerializeField] private CanvasGroup dayChangeGroup;
    [SerializeField] private CanvasGroup feedTimeGroup;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DayManager.Instance.OnDayChanged += HandleDayChanged;
        dayText.text = $"{DayManager.Instance.CurrentDay}";

        if (dayChangeGroup != null)
        {
            dayChangeGroup.alpha = 0f;
            dayChangeGroup.gameObject.SetActive(false);
        }

        if (feedTimeGroup != null)
        {
            feedTimeGroup.alpha = 0f;
            feedTimeGroup.gameObject.SetActive(false);
        }
    }

    public void ShowFeedOverlay()
    {
        if (feedTimeGroup == null) return;
        feedTimeGroup.gameObject.SetActive(true);
        feedTimeGroup.alpha = 1f;
    }

    public void HideFeedOverlay()
    {
        if (feedTimeGroup == null) return;
        feedTimeGroup.alpha = 0f;
        feedTimeGroup.gameObject.SetActive(false);
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
        dayChangeGroup.gameObject.SetActive(false);

        // 7일 간격으로 날씨 룰렛 표시 (시간 정지 유지 → 룰렛 끝난 뒤 복귀)
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;
        if (day > 0 && day % 7 == 0 && WeatherManager.Instance != null)
        {
            bool rouletteDone = false;
            WeatherManager.Instance.ShowRoulette(_ => rouletteDone = true);
            while (!rouletteDone) yield return null;
        }

        Time.timeScale = 1f;
    }
}
