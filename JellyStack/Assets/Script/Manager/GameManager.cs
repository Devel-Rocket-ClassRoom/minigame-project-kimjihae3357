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

    private Vector3 startSpawnPosition = Vector3.zero;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
