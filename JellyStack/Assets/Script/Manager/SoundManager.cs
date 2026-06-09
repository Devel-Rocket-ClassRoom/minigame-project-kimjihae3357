using UnityEngine;

/// <summary>
/// 평상시 BGM ↔ 전투 BGM 자동 전환 + SFX 재생을 담당하는 싱글톤.
/// BGM마다 별도의 AudioSource를 사용 — 사용자가 각 AudioSource 인스펙터에서
/// clip/volume/loop/playOnAwake 등을 자유롭게 설정한다.
/// 이 클래스는 어떤 AudioSource의 .volume도 절대 읽거나 쓰지 않으며,
/// Play() / Stop() / PlayOneShot() 만 호출한다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM AudioSource (BGM마다 별도)")]
    [Tooltip("평상시 BGM 재생용 AudioSource. clip / volume / loop 등은 이 AudioSource 인스펙터에서 직접 설정.")]
    [SerializeField] private AudioSource defaultBgmSource;
    [Tooltip("전투 BGM 재생용 AudioSource. clip / volume / loop 등은 이 AudioSource 인스펙터에서 직접 설정.")]
    [SerializeField] private AudioSource battleBgmSource;

    [Header("SFX AudioSource")]
    [Tooltip("효과음 재생용. PlayOneShot으로 짧은 효과음을 겹쳐 재생. volume은 이 AudioSource 인스펙터에서 설정.")]
    [SerializeField] private AudioSource sfxSource;

    private int activeBattleCount;

    private void Awake()
    {
        // 씬 전환 시 두 번째 인스턴스가 만들어지면 BGM 겹침이 발생할 수 있어 중복 가드.
        // 즉시 GameObject를 비활성화해 자식 AudioSource의 PlayOnAwake가 발동하지 않게 막은 뒤 Destroy.
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // 시작 시 defaultBGM 재생. battleBGM은 무조건 정지 (PlayOnAwake로 켜져 있어도 안전).
        PlayDefaultBGM();
    }

    /// <summary>짧은 효과음을 PlayOneShot으로 재생. 같은 source에서 여러 SFX가 겹쳐도 OK.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        // 어떠한 볼륨 배율도 곱하지 않음. sfxSource.volume + clip 자체 볼륨만 적용.
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>BattlePoint.OnEnable에서 호출 — 첫 전투 시작 시 전투 BGM으로 즉시 전환.</summary>
    public void RegisterBattle()
    {
        activeBattleCount++;
        if (activeBattleCount == 1) PlayBattleBGM();
    }

    /// <summary>BattlePoint.OnDisable에서 호출 — 모든 전투 종료 시 평상시 BGM 복귀.</summary>
    public void UnregisterBattle()
    {
        activeBattleCount = Mathf.Max(0, activeBattleCount - 1);
        if (activeBattleCount == 0) PlayDefaultBGM();
    }

    private void PlayDefaultBGM()
    {
        if (battleBgmSource != null) battleBgmSource.Stop();
        if (defaultBgmSource != null && !defaultBgmSource.isPlaying) defaultBgmSource.Play();
    }

    private void PlayBattleBGM()
    {
        if (defaultBgmSource != null) defaultBgmSource.Stop();
        if (battleBgmSource != null && !battleBgmSource.isPlaying) battleBgmSource.Play();
    }
}
