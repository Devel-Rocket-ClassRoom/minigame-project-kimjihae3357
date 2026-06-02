using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingame : MonoBehaviour
{
    public static UI_Ingame Instance { get; private set; }

    [Header("시간")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Slider timeSlider;
    [Header("날짜")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text nextDayText;
    [Header("날씨")]
    [SerializeField] private Image weatherIcon;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private Sprite sunnyIcon;
    [SerializeField] private Sprite rainIcon;
    [SerializeField] private Sprite snowIcon;
    [SerializeField] private Sprite stormIcon;
    [Tooltip("경고 문구가 완전히 보이는 시간(초).")]
    [SerializeField] private float stormWarningDuration = 2f;
    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup dayChangeGroup;
    [SerializeField] private CanvasGroup feedTimeGroup;
    [SerializeField] private CanvasGroup stormWarningGroup;


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
        {
            WeatherManager.Instance.OnWeatherDetermined += HandleWeatherDetermined;
            WeatherManager.Instance.OnWeatherCleared += HandleWeatherCleared;
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
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherDetermined -= HandleWeatherDetermined;
            WeatherManager.Instance.OnWeatherCleared -= HandleWeatherCleared;
        }
    }

    private void Update()
    {
        int minutes = (int)(DayManager.Instance.ElapsedTime / 60f);
        int seconds = (int)(DayManager.Instance.ElapsedTime % 60f);
        float dayDuration = DayManager.Instance.DayProgress;
        timeText.text = $"{minutes:D2}:{seconds:D2}";
        timeSlider.value = dayDuration;
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
        SetWeatherIcon(weather);
        if (weather == WeatherType.Storm)
            StartCoroutine(StormWarningEffect());
    }

    private void HandleWeatherCleared()
    {
        if (weatherIcon != null) weatherIcon.sprite = emptyIcon;
    }

    private void SetWeatherIcon(WeatherType weather)
    {
        if (weatherIcon == null) return;
        weatherIcon.sprite = weather switch
        {
            WeatherType.Sunny => sunnyIcon,
            WeatherType.Rain  => rainIcon,
            WeatherType.Snow  => snowIcon,
            WeatherType.Storm => stormIcon,
            _                 => emptyIcon,
        };
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

        // 날씨 룰렛 표시 (간격은 GameManager에서 설정)
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;
        if (WeatherManager.Instance != null && GameManager.Instance != null
            && GameManager.Instance.ShouldShowRoulette(day))
        {
            bool rouletteDone = false;
            WeatherManager.Instance.ShowRoulette(_ => rouletteDone = true);
            while (!rouletteDone) yield return null;
        }

        Time.timeScale = 1f;
    }
}
