using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Zoom Setting")]
    [SerializeField] private float zoomSpeed     = 30f;
    [SerializeField] private float smoothness    = 10f;
    [SerializeField] private float minZoom       = 8f;
    [SerializeField] private float maxZoom       = 15f;

    [Header("Pan Setting")]
    [SerializeField] private float panSmoothness = 10f;
    [SerializeField] private LayerMask tableMask = ~0;

    [Header("줌 연출")]
    [Tooltip("숫자가 작을수록 크게 확대")]
    [SerializeField] private float zoomInSize   = 5f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float zoomHoldTime = 1.5f;

    [Header("쉐이크 연출")]
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private float shakeDuration  = 0.25f;

    private Camera cam;
    private float targetZoom;
    private Vector3 targetPosition;

    private bool isPanning;
    private Vector3 panStartWorld;
    private Vector3 panStartCamPos;
    private bool _isShaking;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
        targetPosition = transform.position;
    }

    private void Update()
    {
        CameraZoom();
        HandlePan();
        ApplySmoothing();
    }

    private void CameraZoom()
    {
        if (Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 before = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0));

        targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed * 0.01f, minZoom, maxZoom);

        // 줌 전후 월드 좌표 차이만큼 targetPosition을 보정해 마우스 포인터 기준으로 확대
        float prevSize = cam.orthographicSize;
        cam.orthographicSize = targetZoom;
        Vector3 after = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0));
        cam.orthographicSize = prevSize;

        Vector3 offset = before - after;
        targetPosition += offset;
        if (isPanning) panStartCamPos += offset;
    }

    public void StartPan(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask))
        {
            panStartWorld  = hit.point;
            panStartCamPos = transform.position;
            isPanning      = true;
        }
    }

    private void HandlePan()
    {
        if (!isPanning || Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask))
        {
            Vector3 delta  = panStartWorld - hit.point;
            targetPosition = panStartCamPos + delta;
        }
    }

    public void EndPan()
    {
        isPanning = false;
    }

    public bool IsPanning => isPanning;

    private void ApplySmoothing()
    {
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize, targetZoom, Time.deltaTime * smoothness);

        if (!_isShaking)
            transform.position = Vector3.Lerp(
                transform.position, targetPosition, Time.deltaTime * panSmoothness);
    }

    // ── 포탈 줌 연출 ──────────────────────────────────────────
    public void ZoomToTarget(Vector3 worldPos)
    {
        StartCoroutine(ZoomRoutine(worldPos));
    }

    private IEnumerator ZoomRoutine(Vector3 worldPos)
    {
        Vector3 savedPos  = targetPosition;
        float   savedZoom = targetZoom;

        // 카메라가 현재 화면 중앙에서 바라보는 Y=0 지점 계산
        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 lookAt = centerRay.origin;
        if (Mathf.Abs(centerRay.direction.y) > 0.001f)
        {
            float t = -centerRay.origin.y / centerRay.direction.y;
            lookAt = centerRay.origin + centerRay.direction * t;
        }

        // 포탈을 화면 중앙에 오게 하기 위한 카메라 이동량
        Vector3 offset = new Vector3(worldPos.x - lookAt.x, 0f, worldPos.z - lookAt.z);
        Vector3 camPos = targetPosition + offset;

        DOTween.To(() => targetPosition, x => targetPosition = x, camPos,    zoomDuration);
        DOTween.To(() => targetZoom,     x => targetZoom     = x, zoomInSize, zoomDuration);

        yield return new WaitForSeconds(zoomDuration + zoomHoldTime);

        DOTween.To(() => targetPosition, x => targetPosition = x, savedPos,  zoomDuration);
        DOTween.To(() => targetZoom,     x => targetZoom     = x, savedZoom, zoomDuration);
    }

    // ── 카드 스폰 쉐이크 ──────────────────────────────────────
    public void Shake()
    {
        if (_isShaking) return;
        _isShaking = true;
        transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, shakeStrength), 20, 0f)
                 .OnComplete(() => _isShaking = false);
    }
}
