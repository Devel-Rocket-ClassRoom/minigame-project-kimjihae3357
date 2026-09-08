using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Header("주변 카드 밀어내기")]
    [Tooltip("밀려나는 거리.")]
    [SerializeField] private float battlePushDistance = 2.5f;
    [Tooltip("밀려나는 데 걸리는 시간(초).")]
    [SerializeField] private float battlePushDuration = 0.4f;

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
        var babies = new List<Card>();    // 전투 제외 → 스택에 남김 (아기 주민 + 비전투 카드)
        var fighters = new List<Card>(); // 전투 참여 → BattlePoint로 이동
        foreach (var c in villagerStack.cards)
        {
            if (c is VillagerCard vc)
            {
                var vd = vc.data as VillagerCardData;
                if (vd != null && vd.isBaby)
                    babies.Add(c);   // 아기 주민 → 전투 제외
                else
                    fighters.Add(c); // 성인 주민 → 전투 참여
            }
            else
            {
                babies.Add(c); // 재료 등 비전투 카드 → 전투 제외, 스택에 남김
            }
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
        PushNearbyStacks(mid, battle);
        return true;
    }

    /// <summary>
    /// 전투 시작/합류 위치 주변 카드 스택을 DOTween으로 부드럽게 밀어냄.
    /// 감지 영역은 BattlePoint 콜라이더의 bounds(XZ 평면) 안에 있는 stack만.
    /// 밀어낼 거리/시간은 인스펙터의 distance/duration 사용.
    /// TryStartBattle 및 EnemyCard.JoinExistingBattle 등 외부에서 호출 가능하도록 public.
    /// </summary>
    public void PushNearbyStacks(Vector3 center, BattlePoint exclude)
    {
        if (exclude == null) return;
        var bpCollider = exclude.GetComponentInChildren<Collider>();
        if (bpCollider == null) return;   // 콜라이더 없으면 감지 불가 → 아무것도 안 함

        var allStacks = Object.FindObjectsByType<CardStack>(FindObjectsSortMode.None);
        foreach (var stack in allStacks)
        {
            if (stack == null || stack.IsEmpty) continue;
            if (stack is BattlePoint) continue;
            if (stack.IsDragging) continue;

            Vector3 stackPos = stack.transform.position;
            Vector3 dir = stackPos - center;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) continue;

            // Y는 BattlePoint 콜라이더 중심 Y로 맞춰 XZ 평면 기준으로 Contains 판정
            Vector3 probe = new Vector3(stackPos.x, bpCollider.bounds.center.y, stackPos.z);
            if (!bpCollider.bounds.Contains(probe)) continue;

            Vector3 target = stackPos + dir.normalized * battlePushDistance;
            stack.transform.DOMove(target, battlePushDuration).SetEase(Ease.OutCubic);
        }
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
