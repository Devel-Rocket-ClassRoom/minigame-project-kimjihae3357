using TMPro;
using UnityEngine;

public class VillagerCard : Card
{

    public int CurrentHealth {get; private set;}
    public int Currenthunger {get; private set;}

    private VillagerCardData VillagerData => data as VillagerCardData;
    public int MaxHealth => VillagerData != null ? VillagerData.maxHealth : 0;

    private int birthDay = -1;

    private void Awake()
    {
        InitializeFromData();
    }

    private void Start()
    {
        // 아기 카드만 날짜 변화를 구독
        if (VillagerData != null && VillagerData.isBaby && DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;
    }

    public override void InitializeFromData()
    {
        if (VillagerData == null)
            return;

        CurrentHealth = VillagerData.maxHealth;
        Currenthunger = VillagerData.maxHunger;

        // 아기 카드면 생성 시점의 날짜를 기록
        if (VillagerData.isBaby)
            birthDay = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
    }

    private void HandleDayChanged(int day)
    {
        if (VillagerData == null || !VillagerData.isBaby) return;
        if (VillagerData.adultData == null) return;
        if (day - birthDay < VillagerData.daysToGrow) return;

        GrowUp();
    }

    private void GrowUp()
    {
        if (stack == null || CardSpawner.Instance == null) return;

        // 구독 먼저 해제 (Destroy 전에 안전하게)
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= HandleDayChanged;

        CardStack myStack = stack;
        VillagerCardData adultData = VillagerData.adultData;

        // 아기 카드를 스택에서 제거
        myStack.cards.Remove(this);

        // 성인 카드를 같은 스택에 추가
        CardSpawner.Instance.SpawnIntoStack(adultData, myStack);
        myStack.Refresh();

        Destroy(gameObject);
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

    public void Feed(int amount)
    {
        Currenthunger = Mathf.Max(0, Currenthunger - amount);
        NotifyStatChanged();
    }

    public void Heal(int amount)
    {
        if (VillagerData == null || amount <= 0) return;
        CurrentHealth = Mathf.Min(VillagerData.maxHealth, CurrentHealth + amount);
        NotifyStatChanged();
    }

    public void ResetHunger()
    {
        if (VillagerData == null) return;
        Currenthunger = VillagerData.maxHunger;
        NotifyStatChanged();
    }

}
