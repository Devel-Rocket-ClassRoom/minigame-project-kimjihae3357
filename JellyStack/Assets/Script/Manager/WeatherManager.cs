using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 7일마다 등장하는 날씨 룰렛 UI 컨트롤러.
/// 룰렛 표시 → 무작위 날씨로 회전 후 정지 → 결과 머무름 → 자동 숨김.
/// 결과 날씨 적용 로직은 후속 작업.
/// </summary>
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("룰렛 UI")]
    [Tooltip("룰렛 전체를 감싸는 CanvasGroup (보이기/숨기기 제어).")]
    [SerializeField] private CanvasGroup rouletteGroup;

    [Tooltip("4분할 룰렛 휠 RectTransform. 휠 이미지 배치: 12시=Sunny, 3시=Rain, 6시=Snow, 9시=Storm 이어야 함.")]
    [SerializeField] private RectTransform wheel;

    [Header("회전 설정")]
    [SerializeField] private float spinDuration = 3.0f;
    [Tooltip("스핀 중 추가 전체 회전 수 (보여지는 회전 횟수).")]
    [SerializeField] private int extraSpins = 5;
    [Tooltip("결과 정착 후 룰렛이 화면에 머무는 시간(초).")]
    [SerializeField] private float showResultDuration = 1.2f;

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;

    /// <summary>룰렛이 멈춰 결과가 확정된 시점에 호출 (UI는 아직 화면에 떠 있을 수 있음).</summary>
    public event Action<WeatherType> OnWeatherDetermined;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (rouletteGroup != null)
        {
            rouletteGroup.alpha = 0f;
            rouletteGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 룰렛을 화면에 표시하고 무작위 날씨로 회전시켜 멈춘 뒤, 잠시 결과를 보여주고 숨김.
    /// onComplete는 룰렛이 완전히 사라진 직후 결과와 함께 호출.
    /// </summary>
    public void ShowRoulette(Action<WeatherType> onComplete)
    {
        if (rouletteGroup == null || wheel == null)
        {
            Debug.LogError("[Weather] rouletteGroup 또는 wheel이 인스펙터에 할당되지 않음. WeatherManager 슬롯을 확인하세요.");
            onComplete?.Invoke(CurrentWeather);
            return;
        }

        WeatherType result = (WeatherType)UnityEngine.Random.Range(0, 4);

        // 휠을 시계방향(CW)으로 X도 돌리면 원래 X도 CW 위치에 있던 칸이 아니라
        // (360 - X)도 CW에 있던 칸이 위(12시)로 온다.
        // 결과 칸은 enum 인덱스 × 90도 CW 위치에 그려져 있으므로,
        // 이를 위로 가져오려면 CW (360 - 인덱스×90)도 회전해야 함.
        float targetSector = ((4 - (int)result) % 4) * 90f;
        float total = extraSpins * 360f + targetSector;

        rouletteGroup.gameObject.SetActive(true);
        rouletteGroup.alpha = 1f;
        wheel.localEulerAngles = Vector3.zero;

        wheel.DOLocalRotate(new Vector3(0f, 0f, -total), spinDuration, RotateMode.FastBeyond360)
             .SetEase(Ease.OutCubic)
             .SetUpdate(true)   // Time.timeScale=0 상태에서도 회전
             .OnComplete(() =>
             {
                 CurrentWeather = result;
                 OnWeatherDetermined?.Invoke(result);
                 Debug.Log($"[Weather] 룰렛 결과: {result}");

                 // 결과 칸이 보이도록 잠시 머무른 뒤 자동 숨김
                 DOVirtual.DelayedCall(showResultDuration, () =>
                 {
                     HideRoulette();
                     onComplete?.Invoke(result);
                 }, ignoreTimeScale: true);
             });
    }

    private void HideRoulette()
    {
        if (rouletteGroup == null) return;
        rouletteGroup.alpha = 0f;
        rouletteGroup.gameObject.SetActive(false);
    }
}
