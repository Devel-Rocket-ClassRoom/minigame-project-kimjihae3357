using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 테스트용 적 스폰 헬퍼. 씬에 빈 GameObject 만들어 이 컴포넌트 붙이고
/// EnemyData asset, 스폰 위치, 단축키를 인스펙터에서 지정.
/// Play 중 단축키 누르면 적이 1마리 스폰됨. 인스펙터 우클릭 → "Spawn Enemy"로도 가능.
/// </summary>
public class EnemyTestSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private EnemyCardData enemyData;
    [Tooltip("적이 등장할 월드 좌표.")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(6f, 0f, 6f);

    [Header("단축키")]
    [Tooltip("이 키를 누르면 적 1마리 스폰.")]
    [SerializeField] private Key spawnKey = Key.E;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[spawnKey].wasPressedThisFrame)
        {
            SpawnEnemy();
        }
    }

    [ContextMenu("Spawn Enemy")]
    public void SpawnEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyTestSpawner: enemyData가 설정되지 않았습니다.");
            return;
        }
        if (CardSpawner.Instance == null)
        {
            Debug.LogError("EnemyTestSpawner: CardSpawner.Instance가 없습니다. 씬에 CardSpawner가 있는지 확인.");
            return;
        }

        var card = CardSpawner.Instance.Spawn(enemyData, spawnPosition);
        if (card != null)
            Debug.Log($"[EnemyTestSpawner] {enemyData.cardName} 스폰 완료 at {spawnPosition}");
    }
}
