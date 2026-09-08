using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Card : MonoBehaviour
{
    public CardData data;
    [HideInInspector] public CardStack stack;

    [HideInInspector] public Vector3 targetLocalPosition;
    [HideInInspector] public float followSpeed = 10f;
    [HideInInspector] public bool suppressFollow = false;

    [HideInInspector] public bool IsFrozen = false;   // 눈 날씨: 얼어붙으면 집기/사용 불가

    [Header("그림자")]
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private float shadowGroundY     = 0.02f;  // 그림자가 놓일 월드 Y (지면)
    [SerializeField] private float shadowBaseScale   = 1f;     // 지면에 있을 때 크기
    [SerializeField] private float shadowMinScale    = 0.5f;   // 최대 높이에서 크기
    [SerializeField] private float shadowHeightRange = 1.5f;   // 이 높이(y)에서 min값 도달

    public System.Action OnStatChanged;

    public virtual void InitializeFromData()
    {
    }

    private void Update()
    {
        UpdateShadow();   // suppressFollow 여부와 무관하게 항상 갱신 (DOJump 중에도 동작)

        if (suppressFollow) return;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, targetLocalPosition, Time.deltaTime * followSpeed);
    }

    private void UpdateShadow()
    {
        if (shadowRenderer == null) return;

        // 카드 월드 Y 기준 높이 계산
        float height = Mathf.Max(0f, transform.position.y - shadowGroundY);
        float t = Mathf.Clamp01(height / shadowHeightRange);

        // 크기: 높을수록 작아짐
        float scale = Mathf.Lerp(shadowBaseScale, shadowMinScale, t);
        shadowRenderer.transform.localScale = new Vector3(scale, scale, 1f);

        // 그림자를 항상 지면 Y에 고정 (카드가 올라가도 그림자는 바닥에 붙어있음)
        Vector3 pos = shadowRenderer.transform.position;
        pos.y = shadowGroundY;
        shadowRenderer.transform.position = pos;
    }

    protected void NotifyStatChanged()
    {
        OnStatChanged?.Invoke();
    }

    /// <summary>카드 사망 처리: 소속 스택에서 제거 후 GameObject 파괴.</summary>
    public virtual void Die()
    {
        if (stack != null)
        {
            stack.cards.Remove(this);
            stack.Refresh();
        }
        Destroy(gameObject);
    }
}
