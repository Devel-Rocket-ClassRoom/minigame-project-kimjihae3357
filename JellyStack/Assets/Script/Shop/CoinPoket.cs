using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinPoket : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinText;

    [Header("코인 판별")]
    [Tooltip("코인 카드 판별용 CardData (BuyPoint와 동일한 CoinData.asset 연결)")]
    [SerializeField] private CardData coinData;

    [Header("드롭 영역")]
    [Tooltip("코인이 드롭되는 감지 콜라이더 (BoxCollider 권장, IsTrigger 불필요)")]
    [SerializeField] private Collider dropArea;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [Tooltip("드래그 시작 시 발동할 Animator 트리거 이름")]
    [SerializeField] private string pickupTrigger = "Pickup";
    [Tooltip("드래그 종료 시 발동할 Animator 트리거 이름")]
    [SerializeField] private string dropTrigger = "Drop";
    [Tooltip("코인 넣기/빼기 시 발동할 Animator 트리거 이름")]
    [SerializeField] private string putInTrigger = "PutIn";

    private int coinAmount = 0;

    private void Start()
    {
        UpdateText(); // 초기 표시 "0"
    }

    /// <summary>BuyPoint/SellPoint와 동일한 XZ 영역 체크</summary>
    public bool IsPointInside(Vector3 worldPos)
    {
        if (dropArea == null) return false;
        var b = dropArea.bounds;
        return worldPos.x >= b.min.x && worldPos.x <= b.max.x
            && worldPos.z >= b.min.z && worldPos.z <= b.max.z;
    }

    /// <summary>
    /// 스택에서 코인 카드를 전부 추출해 coinAmount에 합산.
    /// 코인이 1장 이상 있으면 true 반환 — 없으면 false (InputManager가 머지로 흘림).
    /// </summary>
    public bool PutInCoin(CardStack stack)
    {
        if (stack == null || coinData == null) return false;

        var coins = new List<Card>();
        foreach (var c in stack.cards)
            if (c != null && c.data == coinData) coins.Add(c);

        if (coins.Count == 0) return false;

        foreach (var coin in coins)
        {
            stack.cards.Remove(coin);
            Destroy(coin.gameObject);
        }
        stack.Refresh();

        coinAmount += coins.Count;
        OnPutIn();
        UpdateText();
        return true;
    }

    /// <summary>코인 1장을 꺼내 주머니 위치 근처에 DOJump 애니메이션으로 스폰.</summary>
    public void WithdrawCoin()
    {
        if (coinAmount <= 0 || coinData == null || CardSpawner.Instance == null) return;

        OnPutIn();
        CardSpawner.Instance.SpawnNear(coinData, transform.position);
        coinAmount--;
        UpdateText();
    }

    public void OnPickup()
    {
        if (animator != null && !string.IsNullOrEmpty(pickupTrigger))
            animator.SetTrigger(pickupTrigger);
    }

    public void OnPutIn()
    {
        if (animator != null && !string.IsNullOrEmpty(putInTrigger))
            animator.SetTrigger(putInTrigger);
    }

    public void OnDrop()
    {
        if (animator != null && !string.IsNullOrEmpty(dropTrigger))
            animator.SetTrigger(dropTrigger);
    }

    private void UpdateText()
    {
        if (coinText != null)
            coinText.text = coinAmount.ToString();
    }
}
