using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 전투 영역. CardStack을 상속해 카드 관리는 그대로 활용하고,
/// 카드별 공격 코루틴을 돌려 Stacklands 스타일 동시 다발 공격을 처리한다.
/// 카드가 추가되면 SpriteRenderer.size를 키워 빨간 영역이 점점 넓어진다.
/// </summary>
public class BattlePoint : CardStack
{
    [Header("전투 영역")]
    [Tooltip("빨간 사각형을 그리는 SpriteRenderer (자식 'Square'의 컴포넌트). DrawMode=Tiled여야 함.")]
    [SerializeField] private SpriteRenderer areaSprite;
    [SerializeField] private Vector2 baseAreaSize = new Vector2(9f, 9f);
    [Tooltip("카드 한 장 추가될 때마다 영역에 더해질 크기.")]
    [SerializeField] private Vector2 sizePerCard = new Vector2(1f, 0f);

    [Header("배치")]
    [Tooltip("같은 줄 내 카드 사이 간격 (X 축).")]
    [SerializeField] private float horizontalSpacing = 3f;
    [Tooltip("적 줄과 주민 줄 사이의 Z 축 거리 (양수). 화면 위/아래가 반대로 나오면 부호를 뒤집어 주세요.")]
    [SerializeField] private float rowOffset = 2f;

    [Header("전투 종료 처리")]
    [Tooltip("전투 종료 후 생존자가 들어갈 일반 CardStack 프리팹. GameManager의 cardStackPrefab과 동일.")]
    [SerializeField] private GameObject cardStackPrefab;
    [Tooltip("생존자들을 BattlePoint 중심에서 흩뿌릴 반경.")]
    [SerializeField] private float survivorScatterRadius = 2.0f;

    [Header("공격 연출")]
    [Tooltip("공격자가 타겟 위치로 돌진하는 시간(초).")]
    [SerializeField] private float lungeDuration = 0.15f;
    [Tooltip("공격자가 원래 자리로 돌아오는 시간(초).")]
    [SerializeField] private float returnDuration = 0.15f;
    [Tooltip("피격 시 카드 흔들림 지속시간(초).")]
    [SerializeField] private float shakeDuration = 0.3f;
    [Tooltip("피격 흔들림 강도 (XZ 위치 변동 폭).")]
    [SerializeField] private float shakeStrength = 0.3f;
    [Tooltip("타격 순간 타겟 위치에 스폰되는 이펙트 프리팹 (파티클 등). 비워두면 안 스폰.")]
    [SerializeField] private GameObject hitEffectPrefab;

    private readonly Dictionary<Card, Coroutine> _attackRoutines = new Dictionary<Card, Coroutine>();
    private bool _battleStarted;
    private bool _battleEnded;

    public void BeginBattle()
    {
        if (_battleStarted) return;
        _battleStarted = true;

        // 시작 시점의 모든 카드에 대해 공격 코루틴 시작
        var snapshot = new List<Card>(cards);
        foreach (var c in snapshot) StartAttackerCoroutine(c);
        UpdateAreaSize();
        ArrangeBattleCards();
    }

    /// <summary>전투 중 새 카드가 추가될 때도 자동으로 공격 코루틴 시작.</summary>
    public new void AddCard(Card card)
    {
        base.AddCard(card);
        if (_battleStarted && !_battleEnded) StartAttackerCoroutine(card);
        UpdateAreaSize();
        ArrangeBattleCards();
    }

    private void StartAttackerCoroutine(Card c)
    {
        if (c == null) return;
        if (_attackRoutines.ContainsKey(c)) return;
        var routine = StartCoroutine(AttackerRoutine(c));
        _attackRoutines[c] = routine;
    }

    private IEnumerator AttackerRoutine(Card attacker)
    {
        while (!_battleEnded && attacker != null && cards.Contains(attacker))
        {
            float interval = GetAttackInterval(attacker);
            yield return new WaitForSeconds(interval);

            if (_battleEnded) yield break;
            if (attacker == null || !cards.Contains(attacker)) yield break;

            Card target = FindOpposingTarget(attacker);
            if (target == null)
            {
                EndBattle();
                yield break;
            }

            int power = GetAttackPower(attacker);
            // 연출 포함 공격 (돌진 → 데미지 + 흔들림 → 복귀). 공격자가 자기 자리로 돌아올 때까지 대기.
            yield return PlayAttack(attacker, target, power);

            // 사망 정리 후 한쪽 진영이 비면 종료
            if (cards.Count == 0 || !HasBothSides())
            {
                EndBattle();
                yield break;
            }

            // 사망/제거 후 영역/배치 갱신
            UpdateAreaSize();
            ArrangeBattleCards();
        }
    }

    private float GetAttackInterval(Card c)
    {
        if (c is VillagerCard && c.data is VillagerCardData vd)
            return Mathf.Max(0.1f, vd.attackInterval);
        if (c is EnemyCard && c.data is EnemyCardData ed)
            return Mathf.Max(0.1f, ed.attackInterval);
        return 1f;
    }

    private int GetAttackPower(Card c)
    {
        if (c.data is VillagerCardData vd) return vd.attackPower;
        if (c.data is EnemyCardData ed) return ed.attackPower;
        return 0;
    }

    private Card FindOpposingTarget(Card attacker)
    {
        bool attackerIsEnemy = attacker is EnemyCard;
        foreach (var c in cards)
        {
            if (c == null) continue;
            bool isEnemy = c is EnemyCard;
            if (isEnemy != attackerIsEnemy) return c;
        }
        return null;
    }

    private bool HasBothSides()
    {
        bool hasV = false, hasE = false;
        foreach (var c in cards)
        {
            if (c is VillagerCard) hasV = true;
            else if (c is EnemyCard) hasE = true;
            if (hasV && hasE) return true;
        }
        return false;
    }

    /// <summary>
    /// 공격 연출: 공격자가 타겟 위치로 돌진 → 데미지 + 타겟 흔들림 → 공격자 복귀.
    /// 공격자가 복귀할 때까지(코루틴 호출자 입장에서) 블로킹.
    /// </summary>
    private IEnumerator PlayAttack(Card attacker, Card target, int power)
    {
        if (attacker == null || target == null) yield break;

        Vector3 attackerStart = attacker.transform.position;
        Vector3 targetPos = target.transform.position;

        // 1) 돌진 — Card.Update의 lerp가 방해하지 않도록 suppressFollow
        attacker.suppressFollow = true;
        attacker.transform.DOMove(targetPos, lungeDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(lungeDuration);

        // 2) 데미지 + 이펙트 + 타겟 흔들림 (Die 시 타겟이 사라질 수 있음)
        if (target != null && cards.Contains(target))
        {
            // 이펙트는 타겟 위치에 스폰 (타겟이 사망해도 그 자리에서 잠시 보이게 부모 미지정)
            Vector3 hitPos = target.transform.position;
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
            }

            if (target is VillagerCard v) v.TakeDamage(power);
            else if (target is EnemyCard e) e.TakeDamage(power);

            if (target != null && cards.Contains(target))
            {
                ShakeCard(target);
            }
        }

        // 3) 공격자 복귀
        if (attacker != null)
        {
            attacker.transform.DOMove(attackerStart, returnDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (attacker != null) attacker.suppressFollow = false;
                });
            yield return new WaitForSeconds(returnDuration);
        }
    }

    private void ShakeCard(Card target)
    {
        if (target == null) return;
        target.suppressFollow = true;
        target.transform
            .DOShakePosition(shakeDuration,
                             new Vector3(shakeStrength, 0f, 0f),  // X 축(좌우)만 흔들림
                             vibrato: 12,
                             randomness: 0f,                       // 방향 랜덤화 끔 → 좌우로만
                             snapping: false,
                             fadeOut: true)
            .OnComplete(() =>
            {
                if (target != null) target.suppressFollow = false;
            });
    }

    private void EndBattle()
    {
        if (_battleEnded) return;
        _battleEnded = true;

        // 모든 공격 코루틴 정지
        foreach (var kv in _attackRoutines)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        _attackRoutines.Clear();

        // 생존자가 있으면 카드 한 장씩 별도 CardStack에 흩뿌려서 이관 후 BattlePoint 통째로 파괴
        if (cards.Count > 0)
        {
            if (cardStackPrefab != null)
            {
                var survivors = new List<Card>(cards);
                cards.Clear(); // 이중 접근 방지

                int count = survivors.Count;
                for (int i = 0; i < count; i++)
                {
                    var c = survivors[i];
                    if (c == null) continue;

                    // BattlePoint 중심에서 원형으로 흩뿌릴 위치 계산
                    Vector3 offset = ComputeScatterOffset(i, count, survivorScatterRadius);
                    Vector3 spawnPos = transform.position + offset;

                    var newStackGo = Instantiate(cardStackPrefab, spawnPos, Quaternion.identity);
                    var newStack = newStackGo.GetComponent<CardStack>();
                    if (newStack != null)
                    {
                        newStack.AddCard(c);  // 각각 별도 스택에 한 장씩
                    }
                    else
                    {
                        Debug.LogError("BattlePoint: cardStackPrefab에 CardStack 컴포넌트가 없습니다.");
                    }
                }
            }
            else
            {
                Debug.LogError("BattlePoint: cardStackPrefab이 설정되지 않아 생존자를 옮기지 못했습니다. Battle-Point.prefab 인스펙터에서 할당하세요.");
            }
        }

        // 전투 종료 후 씬에 주민이 남았는지 체크 (한 프레임 뒤에 검사 → GameOver 판정)
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.RequestPostBattleCheck();
        }

        Destroy(gameObject);
    }

    /// <summary>n개의 생존자를 BattlePoint 중심 주변 원형으로 분산할 오프셋 계산. n=1이면 중앙.</summary>
    private static Vector3 ComputeScatterOffset(int index, int count, float radius)
    {
        if (count <= 1) return Vector3.zero;
        float angle = (index / (float)count) * Mathf.PI * 2f;
        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private void UpdateAreaSize()
    {
        if (areaSprite == null) return;
        int extra = Mathf.Max(0, cards.Count - 2);
        areaSprite.size = baseAreaSize + sizePerCard * extra;
    }

    /// <summary>위/아래 줄 분리 배치: 적은 윗줄(-Z), 주민은 아랫줄(+Z). 각 줄은 가운데 정렬.</summary>
    private void ArrangeBattleCards()
    {
        // 먼저 적/주민 개수 카운트해서 가운데 정렬 기준 잡기
        int enemyCount = 0;
        int villagerCount = 0;
        foreach (var c in cards)
        {
            if (c is EnemyCard) enemyCount++;
            else villagerCount++;
        }

        int villagerIdx = 0;
        int enemyIdx = 0;
        foreach (var c in cards)
        {
            if (c == null) continue;
            if (c is EnemyCard)
            {
                float x = (enemyIdx - (enemyCount - 1) * 0.5f) * horizontalSpacing;
                c.targetLocalPosition = new Vector3(x, 0f, -rowOffset);   // 윗줄
                enemyIdx++;
            }
            else
            {
                float x = (villagerIdx - (villagerCount - 1) * 0.5f) * horizontalSpacing;
                c.targetLocalPosition = new Vector3(x, 0f, +rowOffset);   // 아랫줄
                villagerIdx++;
            }
            c.followSpeed = 12f;
        }
    }
}
