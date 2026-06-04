using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class EnemyCard : Card
{

    public int CurrentHealth { get; private set; }

    [Header("점프 추적 (프리팹 공용)")]
    [Tooltip("한 번 점프당 전진 거리(유닛). 데이터의 jumpInterval과 조합되어 추적 속도를 결정.")]
    [SerializeField] private float jumpDistance = 2.0f;
    [SerializeField] private float jumpPower = 1.5f;
    [SerializeField] private float jumpDuration = 0.5f;

    private EnemyCardData EnemyData => data as EnemyCardData;
    public int MaxHealth => EnemyData != null ? EnemyData.maxHealth : 0;

    private Coroutine _chaseRoutine;
    private bool _isJumping;

    private void Awake()
    {
        InitializeFromData();
    }

    private void Start()
    {
        _chaseRoutine = StartCoroutine(ChaseRoutine());
    }

    private void OnDestroy()
    {
        if (_chaseRoutine != null) StopCoroutine(_chaseRoutine);
    }

    public override void InitializeFromData()
    {
        if (EnemyData == null)
            return;

        CurrentHealth = EnemyData.maxHealth;
    }

    /// <summary>세이브 복원용 — 저장된 체력으로 덮어쓰기.</summary>
    public void LoadState(int health)
    {
        CurrentHealth = health;
        NotifyStatChanged();
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        NotifyStatChanged();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        // base.Die() 이후엔 stack=null이 되므로 먼저 캡처
        Vector3 deathPos = transform.position;
        CardStack deathStack = stack;

        base.Die();  // stack에서 제거 + Destroy(gameObject) 예약

        var enemyData = data as EnemyCardData;
        if (enemyData == null || enemyData.dropTable == null) return;
        if (CardSpawner.Instance == null) return;

        foreach (var entry in enemyData.dropTable)
        {
            if (entry.card == null) continue;
            if (Random.value < entry.chance)
                CardSpawner.Instance.SpawnNear(entry.card, deathPos, deathStack);
        }
    }

    private IEnumerator ChaseRoutine()
    {
        while (true)
        {
            float interval = (EnemyData != null && EnemyData.jumpInterval > 0f)
                ? EnemyData.jumpInterval
                : 2f;
            yield return new WaitForSeconds(interval);

            if (_isJumping) continue;
            if (stack == null) continue;            // 스택 없으면 (0,0,0) 빨림 회피
            if (stack is BattlePoint) continue;     // 이미 전투 중이면 점프 정지
            if (stack.IsDragging) continue;          // 플레이어 드래그 중엔 쉬기
            if (jumpDistance <= 0f) continue;

            VillagerCard target = FindNearestVillager();
            if (target == null) continue;

            Vector3 myPos = stack.transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 dir = new Vector3(targetPos.x - myPos.x, 0f, targetPos.z - myPos.z);
            float distance = dir.magnitude;
            if (distance < 0.05f) continue;

            float step = Mathf.Min(jumpDistance, distance);
            Vector3 landing = myPos + dir.normalized * step;

            _isJumping = true;
            // 카드 비주얼이 자연스럽게 따라오도록 스택 자체를 점프시킴
            VillagerCard victim = target; // 클로저 캡처
            stack.transform
                .DOJump(landing, jumpPower, 1, jumpDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _isJumping = false;
                    TryEngageBattle(victim);
                });
        }
    }

    private void TryEngageBattle(VillagerCard victim)
    {
        if (victim == null || victim.stack == null) { Debug.Log("[Engage] victim or victim.stack null"); return; }
        if (stack == null) { Debug.Log("[Engage] self.stack null"); return; }
        // 자기 자신이 이미 BattlePoint 안이면 더 할 일 없음
        if (stack is BattlePoint) { Debug.Log("[Engage] self already in BattlePoint"); return; }

        var bm = BattleManager.Instance;
        if (bm == null) { Debug.LogWarning("[Engage] BattleManager.Instance == null — 씬에 BattleManager 오브젝트가 있는지 확인."); return; }

        // 카드 위치 기준 거리 (BattlePoint의 stack 원점은 영역 중심이라 row offset만큼 떨어져 있어 부정확)
        float dist = Vector3.Distance(stack.transform.position, victim.transform.position);
        Debug.Log($"[Engage] dist={dist:F2}, contactRange={bm.ContactRange}");
        if (dist > bm.ContactRange) { Debug.Log("[Engage] out of range — contactRange를 키우거나 jumpDistance를 늘려보세요."); return; }

        // 타겟 주민이 이미 BattlePoint 안이면 그곳에 합류
        if (victim.stack is BattlePoint existingBP)
        {
            Debug.Log("[Engage] joining existing BattlePoint");
            JoinExistingBattle(existingBP);
            return;
        }

        Debug.Log("[Engage] calling TryStartBattle");
        bm.TryStartBattle(stack, victim.stack);
    }

    /// <summary>
    /// 자기가 속한 스택의 모든 카드를 기존 BattlePoint로 이전 후 원본 스택 파괴.
    /// BattlePoint.AddCard가 호출되면서 공격 코루틴 시작 + 영역 확장 + 배치 갱신 자동 처리.
    /// </summary>
    private void JoinExistingBattle(BattlePoint bp)
    {
        if (bp == null || stack == null) return;
        var sourceStack = stack;

        var cardsToMove = new List<Card>(sourceStack.cards);
        sourceStack.cards.Clear();
        foreach (var c in cardsToMove)
        {
            if (c == null) continue;
            bp.AddCard(c);
        }

        Destroy(sourceStack.gameObject);
    }

    private VillagerCard FindNearestVillager()
    {
        VillagerCard nearest = null;
        float nearestDistSqr = float.MaxValue;
        Vector3 myPos = stack != null ? stack.transform.position : transform.position;

        foreach (var v in Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None))
        {
            if (v == null || v.IsFrozen) continue;   // 얼어붙은 주민은 추적/전투 대상 제외
            float d = (v.transform.position - myPos).sqrMagnitude;
            if (d < nearestDistSqr)
            {
                nearestDistSqr = d;
                nearest = v;
            }
        }
        return nearest;
    }
}
