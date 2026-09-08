using TMPro;
using UnityEngine;

/// <summary>
/// 코인만 들어있는 CardStack을 드래그할 때 스택 위에 "x N" 팝업을 띄운다.
/// CardStack prefab의 자식으로 들어가는 형태이며, GetComponentInParent로 부모의 CardStack을 찾는다.
/// 코인 아이콘은 UI에 직접 배치 (스크립트에서 참조 안 함). 텍스트만 동적으로 갱신.
/// </summary>
public class UI_CoinCountPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text countText;
    [Tooltip("코인 카드 판별용 CardData 에셋. SettlementManager의 coinCardData와 동일한 것을 연결.")]
    [SerializeField] private CardData coinCardData;

    private CardStack stack;

    private void Awake()
    {
        // 자식 GameObject로 부착되므로 부모 계층을 거슬러 올라가 CardStack을 찾는다.
        stack = GetComponentInParent<CardStack>();
        if (root != null) root.SetActive(false);
    }

    private void Update()
    {
        bool shouldShow = stack != null && stack.IsDragging && IsAllCoinStack();

        if (shouldShow)
        {
            if (countText != null) countText.text = $"x {stack.cards.Count}";
            if (root != null && !root.activeSelf) root.SetActive(true);
        }
        else if (root != null && root.activeSelf)
        {
            root.SetActive(false);
        }
    }

    /// <summary>스택의 모든 카드가 coinCardData를 사용하는지 검사 (코드베이스 컨벤션).</summary>
    private bool IsAllCoinStack()
    {
        if (coinCardData == null) return false;
        if (stack.cards == null || stack.cards.Count == 0) return false;
        foreach (var c in stack.cards)
        {
            if (c == null || c.data != coinCardData) return false;
        }
        return true;
    }
}
