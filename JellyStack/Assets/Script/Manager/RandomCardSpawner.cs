using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 특정 카드를 N일 간격으로 일정 확률에 따라 콜라이더 영역 내 랜덤 위치에 드롭한다.
/// DayManager.OnDayChanged 시점에 트리거 (FeedPhase / SettlementPhase 끝나고 새 날 시작 직후).
/// 인스펙터의 drops 리스트에 카드/간격/확률/영역을 묶어서 여러 종류 동시 운영 가능.
/// </summary>
public class RandomCardSpawner : MonoBehaviour
{
    public static RandomCardSpawner Instance { get; private set; }

    [System.Serializable]
    public class DropEntry
    {
        [Tooltip("드롭할 카드 데이터.")]
        public CardData card;

        [Tooltip("스폰 간격(일). 1=매일, 3=3·6·9... (day % intervalDays == 0인 날에 시도).")]
        public int intervalDays = 3;

        [Range(0f, 1f), Tooltip("간격 도달 시 실제 스폰될 확률 (0~1).")]
        public float chance = 0.5f;

        [Tooltip("스폰 가능 영역 콜라이더 목록. 랜덤으로 하나 선택 후 그 영역 bounds 내 랜덤 XZ 위치.")]
        public List<Collider> spawnAreas;
    }

    [Header("드롭 설정")]
    [SerializeField] private List<DropEntry> drops = new List<DropEntry>();

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
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += HandleDayChanged;
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
    }

    private void HandleDayChanged(int day)
    {
        if (drops == null) return;

        foreach (var entry in drops)
        {
            if (entry == null || entry.card == null) continue;
            if (entry.intervalDays <= 0) continue;          // 간격 미설정 시 비활성
            if (day % entry.intervalDays != 0) continue;    // N일마다 트리거
            if (Random.value > entry.chance) continue;      // 확률 추첨

            if (!TryPickPosition(entry.spawnAreas, out Vector3 pos)) continue;

            CardSpawner.Instance?.Spawn(entry.card, pos);
        }
    }

    /// <summary>영역 콜라이더 중 하나를 랜덤 선택 후 그 bounds의 XZ 평면 내 랜덤 위치.</summary>
    private bool TryPickPosition(List<Collider> areas, out Vector3 pos)
    {
        pos = default;
        if (areas == null || areas.Count == 0) return false;

        var col = areas[Random.Range(0, areas.Count)];
        if (col == null) return false;

        var b = col.bounds;
        pos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            0f,
            Random.Range(b.min.z, b.max.z)
        );
        return true;
    }
}
