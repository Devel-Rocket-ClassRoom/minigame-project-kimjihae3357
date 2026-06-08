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
        // 씬 전환 시 BGM이 끊기지 않도록 DontDestroyOnLoad + 싱글톤 중복 가드.
        // Title 씬에 한 번 인스턴스화되면 Ingame 씬으로 넘어가도 유지되며,
        // Ingame 씬에 또 다른 SoundManager가 배치되어 있더라도 그 새 인스턴스는 여기서 자동 파괴된다.
        if (Instance != null && Instance != this)
        {
            // 자기 자신의 AudioSource들이 PlayOnAwake로 한 프레임이라도 BGM을 재생해
            // 기존 Instance와 음이 겹쳐 들리는 현상이 있다. Destroy는 다음 프레임 끝에
            // 실제 파괴되므로, 그 사이의 겹침을 막기 위해 즉시 모두 Stop.
            foreach (var src in GetComponentsInChildren<AudioSource>(true))
                if (src != null) src.Stop();

            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 안전망:
        // 슬롯이 NULL이거나 외부 씬 객체(예: 카메라)를 가리키면 씬 전환 시 그 AudioSource가 파괴되어
        // 슬롯이 missing reference 상태가 된다 (PlaySFX의 null 가드에 걸려 무음).
        // → 자기 자신의 자식 AudioSource로 강제 교체. 자식에 없으면 생성.
        EnsureSelfOwnedAudioSource(ref bgmSource, "BGMSource", loop: true);
        EnsureSelfOwnedAudioSource(ref sfxSource, "SFXSource", loop: false);

        if (bgmSource != null) _baseVolume = bgmSource.volume;
    }

    /// <summary>
    /// AudioSource 슬롯이 null이거나 SoundManager 자기 자식이 아닐 경우 자기 자식의 AudioSource로 교체.
    /// 자식에 같은 이름의 GameObject가 있으면 그것을 재사용, 없으면 새로 생성.
    /// 씬 전환 시 외부 AudioSource(예: 카메라에 붙은 것)가 파괴되어 슬롯이 깨지는 것을 방지.
    /// </summary>
    private void EnsureSelfOwnedAudioSource(ref AudioSource slot, string childName, bool loop)
    {
        bool needsReplace = slot == null || !slot.transform.IsChildOf(transform);
        if (!needsReplace) return;

        // 1) 자식에 BGMSource/SFXSource GameObject가 이미 있으면 거기 AudioSource 재사용
        Transform child = transform.Find(childName);
        AudioSource src = child != null ? child.GetComponent<AudioSource>() : null;

        // 2) 없으면 새 자식 GameObject + AudioSource 생성
        if (src == null)
        {
            var go = child != null ? child.gameObject : new GameObject(childName);
            if (child == null) go.transform.SetParent(transform, false);
            src = go.GetComponent<AudioSource>() ?? go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;   // 2D
        }

        slot = src;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // 시작 시 기본 BGM이 재생되도록 보장.
        // (1) clip 슬롯이 비어있거나 다른 곡이면 defaultBGM으로 교체.
        // (2) PlayOnAwake가 꺼져 있어 아직 재생 중이 아니면 명시적으로 Play.
        //    ← 사용자가 인스펙터에서 clip을 미리 defaultBGM으로 설정하고 PlayOnAwake를 끄면
        //       이 명시적 Play가 없으면 영영 BGM이 재생되지 않는다.
        if (bgmSource == null || defaultBGM == null) return;

        if (bgmSource.clip != defaultBGM)
        {
            bgmSource.clip = defaultBGM;
            bgmSource.loop = true;
        }
        if (!bgmSource.isPlaying)
            bgmSource.Play();
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
