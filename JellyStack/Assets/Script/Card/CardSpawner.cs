using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public static CardSpawner Instance {  get; private set; }

    [SerializeField] private GameObject cardStackPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public Card Spawn(CardData data, Vector3 worldPos)
    {
        if (data == null || data.cardPrefab == null)
        {
            Debug.LogError($"Spawn 실패: data 또는 Prefab이 Null");
            return null;
        }

        // 스택 생성
        var stackGo = Instantiate(cardStackPrefab, worldPos, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();

        // 카드 생성
        var cardGo = Instantiate(data.cardPrefab);
        var card = cardGo.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"Spawn 실패: Prefab {data.cardPrefab.name}에 Card 컴포넌트가 없습니다.");
            return null;
        }
        card.data = data;

        stack.AddCard(card);
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

        targetStack.AddCard(card);
        return card;
        }
}
