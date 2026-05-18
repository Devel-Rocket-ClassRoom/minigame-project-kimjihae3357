using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Zoom Setting")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float smoothness;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    [Header("Pan Setting")]
    [SerializeField] private float panSmoothness;

    private Camera cam;
    private float targetZoom;
    private Vector3 targetPosition;

    private bool isPanning;
    private Plane panPlane;
    private Vector3 panStartWorld;
    private Vector3 panStartCamPos;


    private void Awake()
    {
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
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSpeed * 0.01f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }

    public void StartPan(Vector2 screenPosition)
    {
        panPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y - 1f, 0));

        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (panPlane.Raycast(ray, out float distance))
        {
            panStartWorld = ray.GetPoint(distance);
            panStartCamPos = transform.position;
            isPanning = true;
        }
    }

    private void HandlePan()
    {
        if (!isPanning || Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (panPlane.Raycast(ray,out float distance))
        {
            Vector3 currentWorld = ray.GetPoint(distance);
            Vector3 delta = panStartWorld - currentWorld;
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

        transform.position = Vector3.Lerp(
            transform.position, targetPosition, Time.deltaTime * panSmoothness);
    }
}
