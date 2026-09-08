using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WaveSchedule
{
    [Tooltip("몇 일마다 발동. 예: 3 → 3일, 6일, 9일... (0 이하 = 비활성)")]
    public int interval;
    public EnemySpawnerData spawner;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("[시작 아이템]")]
    [SerializeField] private GameObject starterPackPrefab;
    [SerializeField] private GameObject cardStackPrefab;

    [Header("[매니저]")]
    [SerializeField] private UIManager uiManager;

    [Header("날씨 시스템 주기 설정")]
    [Tooltip("몇 일마다 날씨 룰렛을 표시할지. (0 이하 = 비활성)")]
    [SerializeField] private int weatherInterval = 7;

    [Header("적 시스템 주기 설정")]
    [Tooltip("각 항목은 독립적으로 발동. 예: interval=3이면 3일, 6일, 9일마다 소환.")]
    [SerializeField] private List<WaveSchedule> waveSchedules;

    [Header("엔딩")]
    [Tooltip("이 EnemyCardData(예: BossCardData_03)를 가진 적이 처치되면 GameEnding_window가 표시된다. 비워두면 엔딩 트리거 안 됨.")]
    [SerializeField] private EnemyCardData endingBossData;

    private Vector3 startSpawnPosition = Vector3.zero;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 세이브 로드로 진입한 경우 스타터팩을 스폰하지 않음 (SaveManager가 복원).
        if (!SaveSystem.LoadRequested)
            SpawnStarterPack();

        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
    }

    public void GameOver()
    {
        if (uiManager != null)
            uiManager.ShowGameOver();
    }

    /// <summary>주어진 카드 데이터가 엔딩 보스인지 검사. EnemyCard.Die에서 사용.</summary>
    public bool IsEndingBoss(CardData data)
    {
        return endingBossData != null && data == endingBossData;
    }

    /// <summary>엔딩 보스 처치 시 호출. UIManager에 엔딩 창 표시 위임.</summary>
    public void ShowGameEnding()
    {
        if (uiManager != null)
            uiManager.ShowGameEnding();
    }

    /// <summary>날씨 룰렛 발동 여부 판정 — UI_Ingame에서 호출</summary>
    public bool ShouldShowRoulette(int day)
        => day > 0 && weatherInterval > 0 && day % weatherInterval == 0;

    /// <summary>DayChanged 구독 — 각 웨이브 주기 체크</summary>
    private void HandleDayChanged(int day)
    {
        if (waveSchedules == null) return;
        foreach (var ws in waveSchedules)
        {
            if (ws.interval > 0 && day % ws.interval == 0)
                EnemyManager.Instance?.ExecuteWave(ws.spawner);
        }
    }

    private void SpawnStarterPack()
    {
        var packGo = Instantiate(starterPackPrefab, startSpawnPosition, Quaternion.identity);
        var packCard = packGo.GetComponent<PackCard>();
        if (packCard == null) return;

        var stackGo = Instantiate(cardStackPrefab, startSpawnPosition, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();
        stack.AddCard(packCard);
    }
}
