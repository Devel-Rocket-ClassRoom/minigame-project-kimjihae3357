using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("스폰 영역")]
    [Tooltip("포탈이 생성될 수 있는 영역 콜라이더 목록. 스폰 시 랜덤으로 하나 선택 후 그 안에서 위치 결정. 비어있으면 아래 반경 방식 사용.")]
    [SerializeField] private List<Collider> spawnAreas;
    [Tooltip("스폰 영역이 없을 때 사용하는 카메라 중심 가장자리 반경.")]
    [SerializeField] private float spawnRadius = 12f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>GameManager 또는 외부(포털 카드 등)에서 직접 호출. 내부에서 Coroutine으로 처리.</summary>
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
            CameraController.Instance?.ZoomToTarget(pos);
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
                {
                    CardSpawner.Instance.Spawn(enemy, pos);
                    if (spawner.cardSpawnEffectPrefab != null)
                        Instantiate(spawner.cardSpawnEffectPrefab, pos, Quaternion.identity);
                }
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
        // 콜라이더 목록이 있으면 랜덤으로 하나 선택 후 bounds XZ 내 랜덤 위치
        if (spawnAreas != null && spawnAreas.Count > 0)
        {
            Collider col = spawnAreas[UnityEngine.Random.Range(0, spawnAreas.Count)];
            if (col != null)
            {
                var b = col.bounds;
                return new Vector3(
                    UnityEngine.Random.Range(b.min.x, b.max.x),
                    0f,
                    UnityEngine.Random.Range(b.min.z, b.max.z)
                );
            }
        }

        // fallback: 기존 원형 가장자리 방식
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
