using System.Collections.Generic;
using UnityEngine;

// EnemySpawnEntry 별도 정의 불필요 — RandomCardPack.cs의 WeightedCardEntry 재사용
// WeightedCardEntry { CardData data; int weight; }
// EnemyCardData는 CardData를 상속하므로 호환됨

[CreateAssetMenu(fileName = "EnemySpawnerData", menuName = "Card/EnemySpawnerData")]
public class EnemySpawnerData : ScriptableObject
{
    [Header("소환 연출")]
    [Tooltip("스폰 위치에 생성될 포탈 프리팹. UI_CardProgressBar 컴포넌트 포함 시 대기 시간 표시. null이면 연출 없이 즉시 소환.")]
    public GameObject portalPrefab;

    [Tooltip("포탈 생성 후 적 소환까지 대기 시간(초)")]
    public float spawnDelay = 2f;

    [Tooltip("적 카드 한 장이 등장할 때 스폰될 이펙트 프리팹. null이면 이펙트 없음.")]
    public GameObject cardSpawnEffectPrefab;

    [Header("소환 설정")]
    [Tooltip("총 소환 수. 각 슬롯마다 spawnTable 가중치 비율로 적 종류 선택")]
    public int spawnCount = 3;

    [Tooltip("적 종류 + 가중치 목록. EnemyCardData 에셋을 data 슬롯에 연결")]
    public List<WeightedCardEntry> spawnTable;
}
