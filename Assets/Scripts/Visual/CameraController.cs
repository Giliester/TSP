using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public Transform startPosition;
    public Vector2 panLimits = new Vector2(10f, 10f);
    public float zoomSpeed = 5f;
    public float panSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private CinemachinePositionComposer positionComposer;
    private Vector3 lastMousePosition;

    void Start()
    {
        if (virtualCamera == null) return;

        positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
        if (positionComposer == null) return;

        if (startPosition != null)
        {
            positionComposer.Damping = Vector3.zero;
            positionComposer.TargetOffset = startPosition.position;
        }
    }

    void Update()
    {
        if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            HandleZoom();
            HandlePan();
        }
    }

    private void HandleZoom()
    {
        if (positionComposer == null) return;

        Vector2 scroll = Inputs.Instance.Scroll;
        if (scroll.y != 0f)
        {
            var lens = virtualCamera.Lens;
            float direction = Director.Instance.invertZoom ? 1f : -1f;
            lens.OrthographicSize = Mathf.Clamp(
                lens.OrthographicSize + scroll.y * direction * zoomSpeed * Time.deltaTime,
                minZoom,
                maxZoom
            );
            virtualCamera.Lens = lens;
        }
    }

    private void HandlePan()
    {
        if (Inputs.Instance.MiddleClick)
        {
            Vector2 currentMousePosition = Inputs.Instance.Point;

            if (lastMousePosition == Vector3.zero)
            {
                lastMousePosition = currentMousePosition;
                return;
            }

            Vector2 delta = currentMousePosition - (Vector2)lastMousePosition;
            lastMousePosition = currentMousePosition;

            float directionX = Director.Instance.invertPanX ? 1f : -1f;
            float directionZ = Director.Instance.invertPanY ? 1f : -1f;

            var lens = virtualCamera.Lens;
            float proportionalPanSpeed = panSpeed * Mathf.Lerp(0.5f, 2f, (lens.OrthographicSize - minZoom) / (maxZoom - minZoom));

            Vector3 deltaWorld = new Vector3(
                delta.x * directionX,
                0,
                delta.y * directionZ
            ) * proportionalPanSpeed * Time.deltaTime;

            Vector3 newOffset = positionComposer.TargetOffset + deltaWorld;

            if (startPosition != null)
            {
                Vector3 startPos = startPosition.position;
                newOffset.x = Mathf.Clamp(newOffset.x, startPos.x - panLimits.x, startPos.x + panLimits.x);
                newOffset.z = Mathf.Clamp(newOffset.z, startPos.z - panLimits.y, startPos.z + panLimits.y);
            }

            positionComposer.TargetOffset = newOffset;
        }
        else
        {
            lastMousePosition = Vector3.zero;
        }
    }
}