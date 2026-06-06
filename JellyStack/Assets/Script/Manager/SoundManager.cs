using System.Collections;
using UnityEngine;

/// <summary>
/// BGM 매니저. 평상시 BGM과 전투 BGM 사이의 전환을 짧은 페이드로 처리한다.
/// 씬에 BattlePoint가 동시 다발로 생길 수 있으므로 활성 카운터로 추적 —
/// 첫 전투 시작 시 전투 BGM으로, 모든 전투 종료 시 평상시 BGM으로 복귀한다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("재생 대상")]
    [Tooltip("BGM이 흐르는 AudioSource. 보통 카메라에 붙어있는 것을 연결.")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("효과음(SFX) 재생용 AudioSource. PlayOneShot으로 짧은 효과음을 겹쳐 재생. 보통 카메라에 BGM과 별개로 추가.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM 클립")]
    [SerializeField] private AudioClip defaultBGM;
    [SerializeField] private AudioClip battleBGM;

    [Header("전환")]
    [Tooltip("페이드 아웃 / 페이드 인 각각의 길이(초).")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Range(0f, 1f), Tooltip("PlaySFX 호출 시 적용되는 전체 볼륨 배율.")]
    [SerializeField] private float sfxVolumeScale = 1f;

    private int _activeBattleCount;
    private Coroutine _fadeRoutine;
    private float _baseVolume;

    private void Awake()
    {
        Instance = this;
        if (bgmSource != null) _baseVolume = bgmSource.volume;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // 시작 시 기본 BGM이 재생되도록 보장
        if (bgmSource != null && defaultBGM != null && bgmSource.clip != defaultBGM)
        {
            bgmSource.clip = defaultBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    /// <summary>짧은 효과음을 PlayOneShot으로 재생. 같은 source에서 여러 SFX가 겹쳐도 OK.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolumeScale);
    }

    /// <summary>BattlePoint.OnEnable에서 호출 — 첫 전투 시작 시 전투 BGM으로 전환.</summary>
    public void RegisterBattle()
    {
        _activeBattleCount++;
        if (_activeBattleCount == 1)
            SwitchTo(battleBGM);
    }

    /// <summary>BattlePoint.OnDisable에서 호출 — 모든 전투 종료 시 기본 BGM 복귀.</summary>
    public void UnregisterBattle()
    {
        _activeBattleCount = Mathf.Max(0, _activeBattleCount - 1);
        if (_activeBattleCount == 0)
            SwitchTo(defaultBGM);
    }

    private void SwitchTo(AudioClip newClip)
    {
        if (bgmSource == null || newClip == null) return;
        if (bgmSource.clip == newClip) return; // 이미 그 곡이면 무시

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeAndSwitch(newClip));
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip)
    {
        // 1) Fade out — Time.unscaledDeltaTime 사용 (timeScale=0 상태에서도 동작)
        float t = 0f;
        float startVolume = bgmSource.volume;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;

        // 2) Clip 교체 + 재생
        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        // 3) Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, _baseVolume, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = _baseVolume;

        _fadeRoutine = null;
    }
}
