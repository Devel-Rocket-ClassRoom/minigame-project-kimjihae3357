using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float mergeDistance = 2.0f;
    [SerializeField] private float dragYOffset = 1.0f;
    [SerializeField] private GameObject cardStackPrefab;

    private Vector3 stackVelocity;
    private Vector3 lastStackPosition;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController cameraController;

    [Header("Raycast")]
    [SerializeField] private LayerMask cardMask;

    [Header("Pack Settings")]
    [SerializeField] private float dragStartDistance = 10f;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    // 카드 드래그 상태
    private bool isDragging;
    private Plane dragPlane;
    private Vector3 offset;
    private CardStack draggingStack;
    private CardStack sourceStack;

    // 팩 대기 상태
    private PackCard pendingPack;
    private Vector2 packPressStartPos;

    private Vector2 currentPointerPosition;

    // OnClick 콜백은 InputAction 이벤트 처리 단계에서 호출됨 (EventSystem.Update 이전).
    // 그 시점에 IsPointerOverGameObject()를 호출하면 이전 프레임 상태를 읽으므로,
    // 콜백에선 플래그만 세팅하고 실제 처리는 Update에서 수행한다.
    private bool _clickStartedPending;
    private bool _clickCanceledPending;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (cameraController == null && mainCamera != null)
            cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Update()
    {
        // 1. EventSystem-의존 클릭 처리 (Update 시점엔 UI 상태가 이번 프레임 기준으로 갱신돼 안전)
        if (_clickStartedPending)
        {
            _clickStartedPending = false;
            HandleClickStarted();
        }
        if (_clickCanceledPending)
        {
            _clickCanceledPending = false;
            HandleClickCanceled();
        }

        // 2. 드래그/팩 진행 처리
        if (isDragging)
        {
            DragCard();
        }
        else if (pendingPack != null &&
                 (currentPointerPosition - packPressStartPos).magnitude >= dragStartDistance)
        {
            var pack = pendingPack;
            pendingPack = null;
            Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
            StartDragging(pack, ray);
        }
    }

    // PlayerInput (Invoke Unity Events) 콜백 — 포인터 위치 갱신
    public void OnPoint(InputAction.CallbackContext ctx)
    {
        currentPointerPosition = ctx.ReadValue<Vector2>();
    }

    // PlayerInput (Invoke Unity Events) 콜백 — ESC (일시정지 토글)
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (uiManager == null) return;

        CancelOngoingInteractions();
        uiManager.TogglePause();
    }

    // FeedPhase 중 카드 드래그 전체 차단 플래그
    public static bool IsBlocked = false;

    // PlayerInput (Invoke Unity Events) 콜백 — 클릭
    // 실제 처리는 Update의 HandleClickStarted/HandleClickCanceled에서 수행 (UI 체크 정확성 확보)
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _clickStartedPending = true;
        else if (ctx.canceled) _clickCanceledPending = true;
    }

    private void HandleClickStarted()
    {
        if (IsBlocked)
        {
            // FeedPhase 중: SelectFoodCard 전용 클릭 처리
            TrySelectFood();
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        CancelOngoingInteractions();

        if (!TryPickCard())
            cameraController?.StartPan(currentPointerPosition);
    }

    private void HandleClickCanceled()
    {
        // FeedPhase 중엔 release 처리하지 않음 (원본 OnClick의 IsBlocked 분기와 동일하게 early return)
        if (IsBlocked) return;

        if (isDragging)
        {
            ReleaseCard();
        }
        else if (pendingPack != null)
        {
            // holdThreshold 미만 → 짧은 클릭: 카드 한 장 소환
            pendingPack.SpawnNextCard();
            pendingPack = null;
        }
        else
        {
            cameraController?.EndPan();
        }
    }

    private void CancelOngoingInteractions()
    {
        if (isDragging) ReleaseCard();
        pendingPack = null;
        if (cameraController != null && cameraController.IsPanning) cameraController.EndPan();
    }

    // FeedPhase: SelectFoodCard 클릭 감지 (Raycast 전 레이어 무관)
    private void TrySelectFood()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            var indicator = hit.collider.GetComponent<SelectFoodCard>();
            indicator?.OnClick();
        }
    }

    // 카드(팩 포함) 집기
    private bool TryPickCard()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cardMask)) return false;

        var card = hit.collider.GetComponent<Card>();
        if (card == null || card.stack == null) return false;

        // PackCard: holdThreshold 이후 드래그 시작
        if (card is PackCard packCard)
        {
            pendingPack = packCard;
            packPressStartPos = currentPointerPosition;
            return true;
        }

        StartDragging(card, ray);
        return true;
    }

    private void StartDragging(Card card, Ray ray)
    {
        sourceStack = card.stack;

        var moved = sourceStack.SplitFrom(card);
        if (moved == null || moved.Count == 0) return;

        draggingStack = CreateTempStack(moved, card.transform.position);
        draggingStack.IsDragging = true;

        Vector3 elevatedPos = draggingStack.transform.position;
        elevatedPos.y += dragYOffset;
        draggingStack.transform.position = elevatedPos;

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

        if (sourceStack != null && sourceStack.IsEmpty)
            Destroy(sourceStack.gameObject);

        sourceStack = null;
        draggingStack = null;
    }

    private void TryMergeOrDrop()
    {
        if (draggingStack == null) return;

        // 팩 카드는 병합/판매하지 않고 현재 위치에 드롭
        bool isPackStack = draggingStack.cards.Count > 0 && draggingStack.cards[0] is PackCard;

        // 1) SellPoint 우선 판정 (팩 제외)
        if (!isPackStack && SellPoint.Instance != null
            && SellPoint.Instance.IsInRange(draggingStack.transform.position))
        {
            bool nowEmpty = SellPoint.Instance.SellStack(draggingStack);

            if (nowEmpty)
            {
                Destroy(draggingStack.gameObject);
                draggingStack = null;
            }
            else
            {
                // 일부 카드(판매 불가)가 남아있으면 SellPoint 근처에 그대로 드롭
                Vector3 pos = draggingStack.transform.position;
                pos.y -= dragYOffset;
                draggingStack.transform.position = pos;
                draggingStack.Refresh();
            }
            return;
        }

        // 2) 기존 머지/드롭 로직
        CardStack target = isPackStack ? null : FindNearestStack(draggingStack.transform.position, draggingStack);

        if (target != null)
        {
            var cardsToMerge = new List<Card>(draggingStack.cards);
            draggingStack.cards.Clear();
            target.AddCards(cardsToMerge);
            Destroy(draggingStack.gameObject);
        }
        else
        {
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
