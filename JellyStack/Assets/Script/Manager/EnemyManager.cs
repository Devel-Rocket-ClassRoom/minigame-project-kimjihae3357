using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WaveEntry
{
    [Tooltip("이 날짜(OnDayChanged 값)에 발동")]
    public int triggerDay;
    public EnemySpawnerData spawner;
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("웨이브 목록")]
    [Tooltip("날짜 오름차순으로 등록 권장. 같은 날 여러 항목 가능.")]
    [SerializeField] private List<WaveEntry> waves;

    [Header("스폰 위치")]
    [Tooltip("카메라 중심 기준 가장자리 반경. 이 원 둘레에서 랜덤 위치 선택.")]
    [SerializeField] private float spawnRadius = 12f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
    }

    private void HandleDayChanged(int day)
    {
        foreach (var wave in waves)
        {
            if (wave.triggerDay != day) continue;
            ExecuteWave(wave.spawner);
        }
    }

    /// <summary>외부(포털 카드 등)에서 직접 호출 가능. 내부에서 Coroutine으로 처리.</summary>
    public void ExecuteWave(EnemySpawnerData spawner)
    {
        if (spawner == null) return;
        StartCoroutine(SpawnWaveCoroutine(spawner));
    }

    private IEnumerator SpawnWaveCoroutine(EnemySpawnerData spawner)
    {
        if (spawner.spawnTable == null || spawner.spawnTable.Count == 0) yield break;

        Vector3 pos = GetRandomEdgePosition();

        // 포탈 생성
        GameObject portal = null;
        UI_CardProgressBar bar = null;
        if (spawner.portalPrefab != null)
        {
            portal = Instantiate(spawner.portalPrefab, pos, Quaternion.identity);
            bar = portal.GetComponent<UI_CardProgressBar>();
            bar?.Show();
        }

        // 대기 시간 + ProgressBar 갱신
        float elapsed = 0f;
        float delay = Mathf.Max(0f, spawner.spawnDelay);
        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            bar?.SetProgress(elapsed / delay);
            yield return null;
        }

        // 적 소환
        if (CardSpawner.Instance != null)
        {
            for (int i = 0; i < spawner.spawnCount; i++)
            {
                var enemy = PickEnemy(spawner);
                if (enemy != null)
                    CardSpawner.Instance.Spawn(enemy, pos);
            }
        }

        // 포탈 제거
        if (portal != null)
            Destroy(portal);
    }

    /// <summary>WeightedCardEntry 가중치 기반 랜덤 선택 후 EnemyCardData로 캐스팅</summary>
    private EnemyCardData PickEnemy(EnemySpawnerData spawner)
    {
        int totalWeight = 0;
        foreach (var e in spawner.spawnTable)
            if (e.data != null) totalWeight += Mathf.Max(0, e.weight);
        if (totalWeight <= 0) return null;

        int r = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var e in spawner.spawnTable)
        {
            if (e.data == null) continue;
            cumulative += Mathf.Max(0, e.weight);
            if (r < cumulative) return e.data as EnemyCardData;
        }
        return spawner.spawnTable[spawner.spawnTable.Count - 1].data as EnemyCardData;
    }

    private Vector3 GetRandomEdgePosition()
    {
        Camera cam = Camera.main;
        Vector3 center = cam != null
            ? new Vector3(cam.transform.position.x, 0f, cam.transform.position.z)
            : Vector3.zero;

        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return center + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0f,
            Mathf.Sin(angle) * spawnRadius
        );
    }
}
