using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float mergeDistance = 1.0f;
    [SerializeField] private GameObject cardStackPrefab;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private bool isDragging;
    private Plane dragPlane;
    private Vector3 offset;
    private CardStack draggingStack;
    private CardStack sourceStack;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickCard();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragCard();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ReleaseCard();
        }
    }

    // 카드 집기
    private void TryPickCard()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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

        // 드래그 평면(탑다운이므로 XZ평면, normal = Vector3.up)
        dragPlane = new Plane(Vector3.up, draggingStack.transform.position);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            offset = draggingStack.transform.position - mouseWorldPos;
            isDragging = true;
        }

    }

    // 2. 카드 끌기
    private void DragCard()
    {
        if (draggingStack == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            draggingStack.transform.position = mouseWorldPos + offset;
        }

    }

    // 3. 카드 놓기
    private void ReleaseCard()
    {
        isDragging = false;

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
            Vector3 b = new Vector3(s.transform.position.x, 0, s.transform.position.z);
            float dist = Vector3.Distance(a, b);

            if (dist < nearestDist)
            {
                nearest = s;
                nearestDist = dist;
            }
        }
        return nearest;
    }

}
