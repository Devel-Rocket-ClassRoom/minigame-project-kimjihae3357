using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Card : MonoBehaviour
{
    public CardData data;
    [HideInInspector] public CardStack stack;

    [HideInInspector] public Vector3 targetLocalPosition;
    [HideInInspector] public float followSpeed = 10f;
    [HideInInspector] public bool suppressFollow = false;

    public System.Action OnStatChanged;

    public virtual void InitializeFromData()
    {
    }

    private void Update()
    {
        if (suppressFollow) return;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, targetLocalPosition, Time.deltaTime * followSpeed);
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
