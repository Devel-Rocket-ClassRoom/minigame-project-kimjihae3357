using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float mergeDistance = 2.0f; // 스택되는 범위
    [SerializeField] private float dragYOffset = 1.0f;
    [SerializeField] private float dragLagStrength = 5.0f; // 드래그중 딸려가는 강도
    [SerializeField] private GameObject cardStackPrefab;

    private Vector3 stackVelocity;       // 스택의 부드러운 속도
    private Vector3 lastStackPosition;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private bool isDragging;
    private Plane dragPlane;
    private Vector3 offset;

    private CardStack draggingStack;
    private CardStack sourceStack;

    private Vector2 currentPointerPosition;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current != null)
            currentPointerPosition = Mouse.current.position.ReadValue();

        if (isDragging) DragCard();
    }

    // PlayerInput (Invoke Unity Events) 콜백 — 클릭 시작
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TryPickCard();
        }
        else if (ctx.canceled)
        {
            if (isDragging)
            {
                ReleaseCard();
            }
        }
    }


    // 카드 집기
    private void TryPickCard()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length == 0) return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var card = hit.collider.GetComponent<Card>();
            if (card != null && card.stack != null)
            {
                StartDragging(card, ray);
                return;
            }
        }
    }

    private void StartDragging(Card card, Ray ray)
    {
        sourceStack = card.stack;

        // 나 + 내 위 카드들을 떼어내기
        var moved = sourceStack.SplitFrom(card);
        if (moved == null || moved.Count == 0) return;

        // 임시 스택 생성
        draggingStack = CreateTempStack(moved, card.transform.position);

        // 스택을 즉시 올려 모든 카드가 첫 프레임부터 최상위에 표시
        Vector3 elevatedPos = draggingStack.transform.position;
        elevatedPos.y += dragYOffset;
        draggingStack.transform.position = elevatedPos;

        // dragPlane은 원래 카드 Y 기준 (elevation 이전)
        dragPlane = new Plane(Vector3.up, card.transform.position);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            offset = draggingStack.transform.position - mouseWorldPos;
            isDragging = true;
        }

        lastStackPosition = draggingStack.transform.position;
        stackVelocity = Vector3.zero;
    }

    // 카드 끌기
    private void DragCard()
    {
        if (draggingStack == null) return;

        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 newPos = mouseWorldPos + offset;

            draggingStack.transform.position = newPos;

            Vector3 instantVelocity = (newPos - lastStackPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

            // 속도를 부드럽게 보간 → 들쭉날쭉함 제거
            stackVelocity = Vector3.Lerp(stackVelocity, instantVelocity, Time.deltaTime * 10f);

            lastStackPosition = newPos;

            if (draggingStack.cards.Count > 1)
            {
                Vector3 localVelocity = draggingStack.transform.InverseTransformDirection(stackVelocity);

                for (int i = 1; i < draggingStack.cards.Count; i++)
                {
                    Vector3 basePos = new Vector3(0, 0.01f * i, -0.7f * i);
                    draggingStack.cards[i].targetLocalPosition = basePos - localVelocity * i * 0.02f;
                }
            }
        }
    }

    // 카드 놓기
    private void ReleaseCard()
    {
        isDragging = false;

        if (draggingStack == null)
            return;

        TryMergeOrDrop();

        // 원래 떠나온 스택이 비었으면 정리
        if (sourceStack != null && sourceStack.IsEmpty)
        {
            Destroy(sourceStack.gameObject);
        }

        sourceStack = null;
        draggingStack = null;
    }

    private void TryMergeOrDrop()
    {
        if (draggingStack == null) return;

        CardStack target = FindNearestStack(
            draggingStack.transform.position, draggingStack);

        if (target != null)
        {
            var cardsToMerge = new List<Card>(draggingStack.cards);
            draggingStack.cards.Clear();
            target.AddCards(cardsToMerge);

            Destroy(draggingStack.gameObject);
        }
        else
        {
            // 병합 없이 놓을 때 Y를 원래 높이로 되돌려 놓인 카드가 계속 높아지는 걸 방지
            Vector3 pos = draggingStack.transform.position;
            pos.y -= dragYOffset;
            draggingStack.transform.position = pos;
            draggingStack.Refresh();
        }
    }

    private CardStack CreateTempStack(List<Card> moved, Vector3 worldPos)
    {
        GameObject go;
        if (cardStackPrefab != null)
        {
            go = Instantiate(cardStackPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            go = new GameObject("CardStack");
            go.transform.position = worldPos;
            go.AddComponent<CardStack>();
        }

        var newStack = go.GetComponent<CardStack>();
        newStack.AddCards(moved);
        return newStack;
    }

    private CardStack FindNearestStack(Vector3 pos, CardStack exclude)
    {
        CardStack[] allStacks = Object.FindObjectsByType<CardStack>(FindObjectsSortMode.None);

        CardStack nearest = null;
        float nearestDist = mergeDistance;

        foreach (var s in allStacks)
        {
            if (s == exclude) continue;
            if (s.IsEmpty) continue;

            Vector3 a = new Vector3(pos.x, 0, pos.z);
            Vector3 bOrigin = new Vector3(s.transform.position.x, 0, s.transform.position.z);
            Vector3 bTop = new Vector3(s.TopCard.transform.position.x, 0, s.TopCard.transform.position.z);
            float dist = Mathf.Min(Vector3.Distance(a, bOrigin), Vector3.Distance(a, bTop));

            if (dist < nearestDist)
            {
                nearest = s;
                nearestDist = dist;
            }
        }
        return nearest;
    }
}
