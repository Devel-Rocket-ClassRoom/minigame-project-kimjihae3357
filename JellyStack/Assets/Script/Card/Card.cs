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
}
