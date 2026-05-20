using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public static CardSpawner Instance { get; private set; }

    [SerializeField] private GameObject cardStackPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public Card Spawn(CardData data, Vector3 worldPos, Vector3? fromPos = null)
    {
        if (data == null || data.cardPrefab == null)
        {
            Debug.LogError($"Spawn 실패: data 또는 Prefab이 Null");
            return null;
        }

        var stackGo = Instantiate(cardStackPrefab, worldPos, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();

        var cardGo = Instantiate(data.cardPrefab, worldPos, Quaternion.identity);
        var card = cardGo.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"Spawn 실패: Prefab {data.cardPrefab.name}에 Card 컴포넌트가 없습니다.");
            return null;
        }
        card.data = data;
        card.InitializeFromData();

        stack.AddCard(card);

        // fromPos가 있으면 카드를 해당 위치에서 시작시켜 worldPos로 보간되며 날아오게 함
        if (fromPos.HasValue)
            card.transform.position = fromPos.Value;

        return card;
    }

    public Card SpawnIntoStack(CardData data, CardStack targetStack)
    {
        if (data == null || data.cardPrefab == null || targetStack == null)
            return null;

        var cardGo = Instantiate(data.cardPrefab);
        var card = cardGo.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"SpawnIntoStack 실패: Prefab {data.cardPrefab.name}에 Card 컴포넌트가 없습니다.");
            return null;
        }
        card.data = data;
        card.InitializeFromData();

        targetStack.AddCard(card);
        return card;
    }
}
