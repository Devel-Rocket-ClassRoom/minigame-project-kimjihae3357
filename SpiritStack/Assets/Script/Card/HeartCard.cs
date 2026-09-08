using UnityEngine;

public class HeartCard : Card
{
    [Tooltip("이 하트 한 장이 회복시키는 체력. (추후 HeartCardData로 옮길 수 있음)")]
    [SerializeField] private int healAmount = 1;

    public int HealAmount => healAmount;

    private void Awake()
    {
        InitializeFromData();
    }

    public override void InitializeFromData()
    {
        // HeartData에 별도 동적 상태가 없으므로 비워둠.
    }
}
