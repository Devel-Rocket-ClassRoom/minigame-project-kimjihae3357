using UnityEngine;

public class TestSetup : MonoBehaviour
{
    [SerializeField] private GameObject cardStackPrefab;
    [SerializeField] private Card[] cardsToRegister;

    private void Start()
    {
        foreach (var card in cardsToRegister)
        {
            // 카드 위치에 새 스택 생성
            var stackGO = Instantiate(
                cardStackPrefab,
                card.transform.position,
                Quaternion.identity
            );
            var stack = stackGO.GetComponent<CardStack>();

            // 카드를 그 스택에 넣기
            stack.AddCard(card);
        }
    }
}