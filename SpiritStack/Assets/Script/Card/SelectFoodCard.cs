using UnityEngine;

/// <summary>
/// FeedPhase 중 Food 카드 위에 생성되는 선택 인디케이터.
/// SelectCard.prefab에 이 스크립트를 추가하고 Button.onClick → OnClick() 연결.
/// </summary>
public class SelectFoodCard : MonoBehaviour
{
    private FoodCard _food;
    private FeedManager _manager;

    [SerializeField] private float yOffset = 1.5f;

    public void Setup(FoodCard food, FeedManager manager)
    {
        _food = food;
        _manager = manager;
        transform.position = food.transform.position + Vector3.up * yOffset;
    }

    /// <summary>
    /// Button.onClick 이벤트에 연결
    /// </summary>
    public void OnClick()
    {
        if (_food == null || _manager == null) return;
        _manager.OnFoodSelected(_food, this);
    }
}
