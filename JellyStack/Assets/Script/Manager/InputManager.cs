using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

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

    // 드래그 중 미리보기를 보여주고 있는 SellPoint (없으면 null)
    private SellPoint currentHoveredSellPoint;

    // 팩 대기 상태
    private PackCard pendingPack;
    private Vector2 packPressStartPos;

    // CoinPoket 클릭/드래그 상태
    private CoinPoket pendingCoinPoket;
    private bool isDraggingCoinPoket;
    private Plane coinPoketDragPlane;
    private Vector3 coinPoketOffset;
    private float coinPoketOriginalY;
    [SerializeField] private float coinPoketDragYOffset = 2f;

    private Vector2 currentPointerPosition;

    // OnClick 콜백은 InputAction 이벤트 처리 단계에서 호출됨 (EventSystem.Update 이전).
    // 그 시점에 IsPointerOverGameObject()를 호출하면 이전 프레임 상태를 읽으므로,
    // 콜백에선 플래그만 세팅하고 실제 처리는 Update에서 수행한다.
    private bool _clickStartedPending;
    private bool _clickCanceledPending;

    private void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
        if (cameraController == null && mainCamera != null)
            cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        else if (isDraggingCoinPoket)
        {
            DragCoinPoket();
        }
        else if (pendingPack != null &&
                 (currentPointerPosition - packPressStartPos).magnitude >= dragStartDistance)
        {
            var pack = pendingPack;
            pendingPack = null;
            Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
            StartDragging(pack, ray);
        }
        else if (pendingCoinPoket != null &&
                 (currentPointerPosition - packPressStartPos).magnitude >= dragStartDistance)
        {
            StartDraggingCoinPoket();
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

    // FeedPhase 중 카드 드래그 전체 차단 플래그.
    // true로 전환되는 순간 진행 중인 드래그/팩 대기/카메라 팬을 자동 취소한다.
    private static bool _isBlocked;
    public static bool IsBlocked
    {
        get => _isBlocked;
        set
        {
            if (_isBlocked == value) return;
            bool wasJustBlocked = !_isBlocked && value;
            _isBlocked = value;

            // false → true 전환 시점에 자동으로 인터랙션 강제 종료
            if (wasJustBlocked && Instance != null)
            {
                Instance.ForceCancelDrag();
            }
        }
    }

    /// <summary>
    /// 외부 이벤트(FeedPhase 진입 등)로 진행 중인 드래그/팩/팬을 강제 취소.
    /// 드래그 중이던 카드는 원래 sourceStack으로 복귀시킨다.
    /// </summary>
    public void ForceCancelDrag()
    {
        pendingPack = null;
        pendingCoinPoket = null;
        isDraggingCoinPoket = false;

        ClearSellPointHover();

        if (cameraController != null && cameraController.IsPanning)
            cameraController.EndPan();

        if (!isDragging || draggingStack == null)
        {
            sourceStack = null;
            draggingStack = null;
            isDragging = false;
            return;
        }

        isDragging = false;

        if (sourceStack != null)
        {
            // 카드를 원래 스택으로 복귀
            var cardsToReturn = new List<Card>(draggingStack.cards);
            draggingStack.cards.Clear();
            sourceStack.AddCards(cardsToReturn);
            Destroy(draggingStack.gameObject);
        }
        else
        {
            // 소스가 없으면 그냥 현 위치에 떨궈둠
            draggingStack.IsDragging = false;
            Vector3 pos = draggingStack.transform.position;
            pos.y -= dragYOffset;
            draggingStack.transform.position = pos;
            draggingStack.Refresh();
        }

        sourceStack = null;
        draggingStack = null;
    }

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
            // FeedPhase 중: 카드 집기는 막되, 음식 선택 + 카메라 팬은 허용
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (!TrySelectFood())
                cameraController?.StartPan(currentPointerPosition);
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        CancelOngoingInteractions();

        if (TryPickCoinPoket()) return;

        if (!TryPickCard())
            cameraController?.StartPan(currentPointerPosition);
    }

    private void HandleClickCanceled()
    {
        // FeedPhase 중엔 카드 release는 없지만 카메라 팬은 종료시켜야 함
        if (IsBlocked)
        {
            cameraController?.EndPan();
            return;
        }

        if (isDragging)
        {
            ReleaseCard();
        }
        else if (isDraggingCoinPoket)
        {
            // Y 원위치 복구
            if (pendingCoinPoket != null)
            {
                Vector3 pos = pendingCoinPoket.transform.position;
                pos.y = coinPoketOriginalY;
                pendingCoinPoket.transform.position = pos;
                pendingCoinPoket.OnDrop();
            }
            isDraggingCoinPoket = false;
            pendingCoinPoket = null;
        }
        else if (pendingCoinPoket != null)
        {
            pendingCoinPoket.WithdrawCoin();
            pendingCoinPoket = null;
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
    // 음식 인디케이터를 실제로 선택했으면 true 반환 (그 경우 카메라 팬 시작 안 함)
    private bool TrySelectFood()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            var indicator = hit.collider.GetComponentInParent<SelectFoodCard>();
            if (indicator != null)
            {
                indicator.OnClick();
                return true;
            }
        }
        return false;
    }

    // CoinPoket 클릭 감지 — 마우스다운 시 pending 저장, 드래그 거리 초과 시 이동 / 마우스업 시 WithdrawCoin
    private bool TryPickCoinPoket()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return false;
        var pocket = hit.collider.GetComponentInParent<CoinPoket>();
        if (pocket == null) return false;
        pendingCoinPoket = pocket;
        packPressStartPos = currentPointerPosition;   // PackCard와 동일 필드 재사용
        return true;
    }

    private void StartDraggingCoinPoket()
    {
        isDraggingCoinPoket = true;
        pendingCoinPoket.OnPickup();

        // Y 올리기
        coinPoketOriginalY = pendingCoinPoket.transform.position.y;
        Vector3 raisedPos = pendingCoinPoket.transform.position;
        raisedPos.y += coinPoketDragYOffset;
        pendingCoinPoket.transform.position = raisedPos;

        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        coinPoketDragPlane = new Plane(Vector3.up, pendingCoinPoket.transform.position);

        if (coinPoketDragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            coinPoketOffset = pendingCoinPoket.transform.position - mouseWorldPos;
        }
    }

    private void DragCoinPoket()
    {
        if (pendingCoinPoket == null) { isDraggingCoinPoket = false; return; }

        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (coinPoketDragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            pendingCoinPoket.transform.position = mouseWorldPos + coinPoketOffset;
        }
    }

    // 카드(팩 포함) 집기
    private bool TryPickCard()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentPointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cardMask)) return false;

        var card = hit.collider.GetComponent<Card>();
        if (card == null || card.stack == null) return false;

        // 얼어붙은 카드는 집을 수 없음 (눈 날씨)
        if (card.IsFrozen) return false;

        // 적 카드는 플레이어가 잡을수 없음
        if (card is EnemyCard) return false;

        // BattlePoint 안의 카드는 전투 중이므로 픽 차단
        if (card.stack is BattlePoint) return false;

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

        UpdateSellPointHover();
    }

    // 드래그 중인 스택의 현재 위치를 기준으로 SellPoint 호버 상태를 갱신.
    // 가장 가까운 SellPoint 위에 있으면 그 SellPoint의 미리보기를 띄우고, 벗어나면 숨긴다.
    // TryMergeOrDrop의 SellPoint 탐색 로직과 동일한 패턴.
    private void UpdateSellPointHover()
    {
        if (draggingStack == null)
        {
            ClearSellPointHover();
            return;
        }

        // 팩 카드는 판매 대상이 아니므로 미리보기 표시 안 함
        bool isPackStack = draggingStack.cards.Count > 0 && draggingStack.cards[0] is PackCard;
        if (isPackStack)
        {
            ClearSellPointHover();
            return;
        }

        Vector3 dropPos = draggingStack.transform.position;
        Vector3 dropXZ = new Vector3(dropPos.x, 0f, dropPos.z);

        SellPoint nearest = null;
        float bestDistSqr = float.MaxValue;
        var sellPoints = Object.FindObjectsByType<SellPoint>(FindObjectsSortMode.None);
        foreach (var sp in sellPoints)
        {
            if (sp == null || !sp.OverlapsWith(draggingStack)) continue;
            Vector3 spXZ = new Vector3(sp.transform.position.x, 0f, sp.transform.position.z);
            float d = (spXZ - dropXZ).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                nearest = sp;
            }
        }

        if (nearest == currentHoveredSellPoint)
        {
            // 같은 SellPoint 위에 머무는 동안에도 스택 구성이 바뀔 수 있으므로 매 프레임 갱신
            if (nearest != null) nearest.ShowPreview(draggingStack);
            return;
        }

        if (currentHoveredSellPoint != null)
            currentHoveredSellPoint.HidePreview();

        currentHoveredSellPoint = nearest;

        if (currentHoveredSellPoint != null)
            currentHoveredSellPoint.ShowPreview(draggingStack);
    }

    private void ClearSellPointHover()
    {
        if (currentHoveredSellPoint != null)
        {
            currentHoveredSellPoint.HidePreview();
            currentHoveredSellPoint = null;
        }
    }

    // 카드 놓기
    private void ReleaseCard()
    {
        isDragging = false;

        ClearSellPointHover();

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

        Vector3 dropPos = draggingStack.transform.position;
        Vector3 dropXZ = new Vector3(dropPos.x, 0f, dropPos.z);

        // 1) BuyPoint 우선 판정 (팩 제외) — 콜라이더 영역 안에 떨어졌을 때
        //    여러 BuyPoint 콜라이더가 겹친 경우엔 드롭 위치에서 가장 가까운(XZ 거리) BuyPoint를 선택
        if (!isPackStack)
        {
            BuyPoint targetBuyPoint = null;
            float bestDistSqr = float.MaxValue;
            var buyPoints = Object.FindObjectsByType<BuyPoint>(FindObjectsSortMode.None);
            foreach (var bp in buyPoints)
            {
                if (bp == null || !bp.IsPointInside(dropPos)) continue;
                Vector3 bpXZ = new Vector3(bp.transform.position.x, 0f, bp.transform.position.z);
                float d = (bpXZ - dropXZ).sqrMagnitude;
                if (d < bestDistSqr)
                {
                    bestDistSqr = d;
                    targetBuyPoint = bp;
                }
            }

            if (targetBuyPoint != null)
            {
                bool purchased = targetBuyPoint.TryBuy(draggingStack);
                if (purchased)
                {
                    if (draggingStack.IsEmpty)
                    {
                        Destroy(draggingStack.gameObject);
                        draggingStack = null;
                    }
                    else
                    {
                        // 코인 외 잔여 카드는 BuyPoint 근처에 드롭
                        Vector3 pos = draggingStack.transform.position;
                        pos.y -= dragYOffset;
                        draggingStack.transform.position = pos;
                        draggingStack.Refresh();
                    }
                    return;
                }
                // 구매 실패(코인 부족) → 아래 SellPoint/머지 로직으로 흘림
            }
        }

        // 2) SellPoint 판정 (팩 제외) — 콜라이더 영역 안에 떨어졌을 때
        //    여러 SellPoint 콜라이더가 겹친 경우엔 드롭 위치에서 가장 가까운(XZ 거리) SellPoint를 선택
        if (!isPackStack)
        {
            SellPoint targetSellPoint = null;
            float bestSellDistSqr = float.MaxValue;
            var sellPoints = Object.FindObjectsByType<SellPoint>(FindObjectsSortMode.None);
            foreach (var sp in sellPoints)
            {
                if (sp == null || !sp.OverlapsWith(draggingStack)) continue;
                Vector3 spXZ = new Vector3(sp.transform.position.x, 0f, sp.transform.position.z);
                float d = (spXZ - dropXZ).sqrMagnitude;
                if (d < bestSellDistSqr)
                {
                    bestSellDistSqr = d;
                    targetSellPoint = sp;
                }
            }

            if (targetSellPoint != null)
            {
                bool nowEmpty = targetSellPoint.SellStack(draggingStack);

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
        }

        // 3) CoinPoket 판정 (팩 제외) — 코인 카드를 주머니에 넣기
        if (!isPackStack)
        {
            CoinPoket targetCoinPoket = null;
            float bestCoinDistSqr = float.MaxValue;
            var coinPokets = Object.FindObjectsByType<CoinPoket>(FindObjectsSortMode.None);
            foreach (var cp in coinPokets)
            {
                if (cp == null || !cp.IsPointInside(dropPos)) continue;
                Vector3 cpXZ = new Vector3(cp.transform.position.x, 0f, cp.transform.position.z);
                float d = (cpXZ - dropXZ).sqrMagnitude;
                if (d < bestCoinDistSqr) { bestCoinDistSqr = d; targetCoinPoket = cp; }
            }

            if (targetCoinPoket != null)
            {
                bool hadCoins = targetCoinPoket.PutInCoin(draggingStack);
                if (hadCoins)
                {
                    if (draggingStack.IsEmpty)
                    {
                        Destroy(draggingStack.gameObject);
                        draggingStack = null;
                    }
                    else
                    {
                        // 비-코인 잔여 카드는 CoinPoket 근처에 그대로 드롭
                        Vector3 pos = draggingStack.transform.position;
                        pos.y -= dragYOffset;
                        draggingStack.transform.position = pos;
                        draggingStack.Refresh();
                    }
                    return;
                }
                // 코인 없음(hadCoins=false) → 아래 머지 로직으로 흘림
            }
        }

        // 4) 기존 머지/드롭 로직
        CardStack target = isPackStack ? null : FindNearestStack(draggingStack.transform.position, draggingStack);

        if (target != null)
        {
            // 머지 직전 draggingStack을 dragYOffset만큼 내림.
            // AddCards/AddCard는 SetParent(worldPositionStays=true)라 카드 world position을 유지하므로,
            // 부모를 먼저 내려놓으면 자식 카드들도 자연스럽게 내려간 위치에서 target stack에 합류 → 들린 채 고정되는 버그 방지.
            Vector3 dropPosLow = draggingStack.transform.position;
            dropPosLow.y -= dragYOffset;
            draggingStack.transform.position = dropPosLow;

            var cardsToMerge = new List<Card>(draggingStack.cards);
            draggingStack.cards.Clear();

            if (target is BattlePoint bp)
            {
                // BattlePoint는 한 장씩 addCard 호출해야 BattlePoint.AddCard(new hide)가 실행되야
                // 공격 코루틴 시작 + ArrangeBattleCard + 영역 확장이 일어남
                foreach (var c in cardsToMerge)
                {
                    bp.AddCard(c);
                }
            }
            else
            {
                target.AddCards(cardsToMerge);
            }

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

            // 얼어붙은 카드가 든 스택은 머지 대상 제외 (얼린 카드는 항상 단일 스택)
            if (s.BottomCard != null && s.BottomCard.IsFrozen) continue;

            // BattlePoint는 항상 머지 허용 (전투 참전). 그 외 스택은 적 포함 시 제외.
            if (!(s is BattlePoint) && ContainsEnemy(s)) continue;

            // 비주민 카드를 주민 위에 올릴 수 없음 (주민은 항상 최상단)
            if (s.TopCard is VillagerCard
                && draggingStack.cards.Count > 0
                && !(draggingStack.cards[0] is VillagerCard))
                continue;

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

    private bool ContainsEnemy(CardStack stack)
    {
        foreach (var c in stack.cards)
            if (c is EnemyCard) return true;
        return false;
    }
}
