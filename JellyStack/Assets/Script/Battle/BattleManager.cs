using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 시작 진입점. 적이 주민에게 도착했을 때 호출되어 BattlePoint를 생성하고
/// 양쪽 스택의 카드를 BattlePoint 안으로 이동시킨다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("프리팹")]
    [Tooltip("BattlePoint(=CardStack 상속) 컴포넌트를 가진 Battle-Point.prefab.")]
    [SerializeField] private GameObject battlePointPrefab;

    [Header("전투 시작 조건")]
    [Tooltip("적 점프 후 이 거리 이내 Villager가 있으면 전투 시작 (XZ 거리).")]
    [SerializeField] private float contactRange = 1.5f;
    public float ContactRange => contactRange;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool TryStartBattle(CardStack enemyStack, CardStack villagerStack)
    {
        if (enemyStack == null || villagerStack == null) { Debug.Log("[Battle] stack null"); return false; }
        if (enemyStack == villagerStack) { Debug.Log("[Battle] same stack"); return false; }
        if (battlePointPrefab == null)
        {
            Debug.LogError("[Battle] battlePointPrefab이 설정되지 않았습니다. BattleManager 인스펙터 슬롯을 확인하세요.");
            return false;
        }

        // 두 스택의 중간 지점에 BattlePoint 생성
        Vector3 mid = (enemyStack.transform.position + villagerStack.transform.position) * 0.5f;
        var bpGo = Instantiate(battlePointPrefab, mid, Quaternion.identity);
        var battle = bpGo.GetComponent<BattlePoint>();
        if (battle == null)
        {
            Debug.LogError("[Battle] battlePointPrefab에 BattlePoint 컴포넌트가 없습니다. Battle-Point.prefab에 BattlePoint 스크립트를 추가하세요.");
            Destroy(bpGo);
            return false;
        }
        Debug.Log($"[Battle] BattlePoint 생성 at {mid}, 카드 {enemyStack.cards.Count + villagerStack.cards.Count}장 이동");

        // 적 카드는 전부 전투에 참여
        var moved = new List<Card>();
        moved.AddRange(enemyStack.cards);
        enemyStack.cards.Clear();

        // 주민 카드 중 아기(isBaby)는 전투에서 제외
        var babies = new List<Card>();
        var fighters = new List<Card>();
        foreach (var c in villagerStack.cards)
        {
            var vd = (c is VillagerCard) ? (c.data as VillagerCardData) : null;
            if (vd != null && vd.isBaby)
                babies.Add(c);
            else
                fighters.Add(c);
        }
        moved.AddRange(fighters);
        villagerStack.cards.Clear();

        battle.AddCards(moved);
        Destroy(enemyStack.gameObject);

        // 아기가 있으면 기존 스택에 남겨둠, 없으면 빈 스택 파괴
        if (babies.Count > 0)
        {
            foreach (var b in babies)
                villagerStack.cards.Add(b);
            villagerStack.Refresh();
        }
        else
        {
            Destroy(villagerStack.gameObject);
        }

        battle.BeginBattle();
        return true;
    }

    /// <summary>
    /// 전투 종료 직후 호출. 한 프레임 기다린 뒤 씬에 남은 VillagerCard 수를 세고
    /// 0이면 GameOver. BattlePoint 자신은 Destroy되므로 코루틴은 BattleManager가 들고 있어야 함.
    /// </summary>
    public void RequestPostBattleCheck()
    {
        StartCoroutine(PostBattleCheckRoutine());
    }

    private IEnumerator PostBattleCheckRoutine()
    {
        // Destroy()는 이번 프레임 끝에 적용되므로 한 프레임 기다려야 정확한 카운트가 됨
        yield return null;

        var villagers = Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None);
        int alive = 0;
        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i] != null) alive++;
        }

        if (alive == 0)
        {
            Debug.Log("[Battle] 전투 종료 후 살아있는 주민이 없음 → GameOver");
            GameManager.Instance?.GameOver();
        }
    }
}
