using UnityEngine;

public class FoodCard : Card
{
    public int currentFullness { get; private set;  }

    private FoodCardData FoodData => data as FoodCardData;

    private void Awake()
    {
        currentFullness = FoodData.maxFullness;
    }

    public void Consume(int amount)
    {
        currentFullness -= amount;
        NotifyStatChanged();
    }
}
