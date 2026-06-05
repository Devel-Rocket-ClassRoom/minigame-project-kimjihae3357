using UnityEngine;

public class FoodCard : Card
{
    public int CurrentFullness { get; private set;  }

    private FoodCardData FoodData => data as FoodCardData;

    private void Awake()
    {
        InitializeFromData();
    }

    public override void InitializeFromData()
    {
        if (FoodData != null)
            CurrentFullness = FoodData.maxFullness;
    }

    public void Consume(int amount)
    {
        CurrentFullness -= amount;
        NotifyStatChanged();
    }

    /// <summary>세이브 복원용 — 저장된 포만도로 덮어쓰기.</summary>
    public void LoadState(int fullness)
    {
        CurrentFullness = fullness;
        NotifyStatChanged();
    }
}
