using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float mergeDistance = 2.0f; // 스택되는 범위
    [SerializeField] private float dragYOffset = 1.0f;
    [SerializeField] private GameObject cardStackPrefab;

    private Vector3 stackVelocity;       // 스택의 부드러운 속도
    private Vector3 lastStackPosition;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController cameraController;

    [Header("Raycast")]
    [SerializeField] private LayerMask cardMask;

    private bool isDragging;
    private Plane dragPlane;
    private Vector3 offset;

    private CardStack draggingStack;
    private CardStack sourceStack;

    private Vector2 currentPointerPosition;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (cameraController == null && mainCamera != null)
            cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Update()
    {
        if (isDragging) DragCard();
    }

    // PlayerInput (Invoke Unity Events) 콜백 — 포인터 위치 갱신
    public void OnPoint(InputAction.CallbackContext ctx)
    {
        currentPointerPosition = ctx.ReadValue<Vector2>();
    }

    // PlayerInput (Invoke Unity Events) 콜백 — 클릭 시작
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            CancelOngoingInteractions();

            if (!TryPickCard())
                cameraController?.StartPan(currentPointerPosition);
        }
        else if (ctx.canceled)
        {
            if (isDragging)
                ReleaseCard();
            else
                cameraController?.EndPan();
        }
    }

    private void CancelOngoingInteractions()
    {
        if (isDragging) ReleaseCard();
        if (cameraController != null && cameraController.IsPanning) cameraController.EndPan();
    }

    
    // 카드 집기
    private bool TryPickCard()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cardMask)) return false;

        var card = hit.collider.GetComponent<Card>();
        if (card == null || card.stack == null) return false;

        StartDragging(card, ray);
        return true;
    }
    

    private void StartDragging(Card card, Ray ray)
    {
        sourceStack = card.stack;

        // 나 + 내 위 카드들을 떼어내기
        var moved = sourceStack.SplitFrom(card);
        if (moved == null || moved.Count == 0) return;

        // 임시 스택 생성
        draggingStack = CreateTempStack(moved, card.transform.position);
        draggingStack.IsDragging = true;

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

        if (draggingStack != null)
            draggingStack.IsDragging = false;

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
