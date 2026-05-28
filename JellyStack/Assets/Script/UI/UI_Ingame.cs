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
    [SerializeField] private CanvasGroup stormWarningGroup;
    [Tooltip("경고 문구가 완전히 보이는 시간(초).")]
    [SerializeField] private float stormWarningDuration = 2f;

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

        if (stormWarningGroup != null)
        {
            stormWarningGroup.alpha = 0f;
            stormWarningGroup.gameObject.SetActive(false);
        }

        if (WeatherManager.Instance != null)
            WeatherManager.Instance.OnWeatherDetermined += HandleWeatherDetermined;
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
        if (WeatherManager.Instance != null)
            WeatherManager.Instance.OnWeatherDetermined -= HandleWeatherDetermined;
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

    private void HandleWeatherDetermined(WeatherType weather)
    {
        if (weather == WeatherType.Storm)
            StartCoroutine(StormWarningEffect());
    }

    private IEnumerator StormWarningEffect()
    {
        if (stormWarningGroup == null) yield break;

        // 룰렛 UI가 닫히길 기다린 뒤 표시 (showResultDuration 이후 rouletteDone=true → timeScale=1)
        // timeScale이 0인 동안 룰렛이 보이므로, 1이 된 직후 경고 등장
        yield return new WaitUntil(() => Time.timeScale > 0f);

        stormWarningGroup.gameObject.SetActive(true);
        stormWarningGroup.alpha = 1f;

        yield return new WaitForSeconds(stormWarningDuration);

        // 1초에 걸쳐 페이드 아웃
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            stormWarningGroup.alpha = 1f - t;
            yield return null;
        }

        stormWarningGroup.alpha = 0f;
        stormWarningGroup.gameObject.SetActive(false);
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
