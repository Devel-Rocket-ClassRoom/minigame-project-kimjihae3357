using UnityEngine;

public class FoodCard : Card
{
    public int CurrentFullness { get; private set;  }

    private FoodCardData FoodData => data as FoodCardData;

    private void Awake()
    {
        CurrentFullness = FoodData.maxFullness;
    }

    public void Consume(int amount)
    {
        CurrentFullness -= amount;
        NotifyStatChanged();
    }
}
