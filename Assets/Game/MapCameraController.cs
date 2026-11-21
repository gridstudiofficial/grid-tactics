using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    public static MapCameraController Instance { get; private set; }

    [Header("Límites del Mapa (en Coordenadas de Casilla)")]
    [SerializeField] private Vector2Int mapMinBounds = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int mapMaxBounds = new Vector2Int(20, 15);
    [SerializeField] private float cellSize = 1f;

    public Vector2Int MapMinBounds => mapMinBounds;
    public Vector2Int MapMaxBounds => mapMaxBounds;

    [Header("Zoom de Cámara")]
    [SerializeField] private float minOrthographicSize = 3f;  // Zoom in máximo (más cerca)
    [SerializeField] private float initialOrthographicSize = 7f; // Zoom inicial de partida
    [SerializeField] private bool useCurrentSizeAsInitial = true;
    [SerializeField] private float scrollZoomStep = 4f;
    [SerializeField] private float keyZoomSpeed = 8f;
    [SerializeField] private float zoomSmoothSpeed = 12f;
    [SerializeField] private bool fitFullMapAtMaxZoomOut = true;
    [SerializeField] private float manualMaxOrthographicSize = 12f;

    [Header("Movimiento por Borde con el Cursor")]
    [SerializeField] private int edgeTriggerTiles = 2;
    [SerializeField] private float edgePanSpeed = 8f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Seguimiento de Fichas")]
    [SerializeField] private float focusSnapSpeed = 12f;

    private Camera cam;
    private float targetOrthographicSize;
    private bool isFullMapViewActive = false;
    private Vector3 desiredPosition;
    private Unit unitToFollow = null;
    private bool isFollowingUnit = false;

    private Vector3 positionBeforeFullMapView;
    private bool hasSavedPositionBeforeFullMapView = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        cam = GetComponent<Camera>();
        cam.orthographic = true;

        float maxAllowed = GetMaxAllowedOrthographicSize();
        if (useCurrentSizeAsInitial && cam.orthographicSize > 0.01f)
        {
            initialOrthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxAllowed);
        }
        else
        {
            initialOrthographicSize = Mathf.Clamp(initialOrthographicSize, minOrthographicSize, maxAllowed);
        }

        targetOrthographicSize = initialOrthographicSize;
        cam.orthographicSize = targetOrthographicSize;

        desiredPosition = ClampCameraPosition(transform.position);
        transform.position = desiredPosition;
    }

    private void Update()
    {
        HandleZoomInput();
        HandleToggleFullMapView();
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        float maxAllowedSize = GetMaxAllowedOrthographicSize();
        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthographicSize, maxAllowedSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, zoomSmoothSpeed * Time.unscaledDeltaTime);

        if (isFollowingUnit && unitToFollow != null)
        {
            desiredPosition = new Vector3(unitToFollow.transform.position.x, unitToFollow.transform.position.y, transform.position.z);
            desiredPosition = ClampCameraPosition(desiredPosition);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, focusSnapSpeed * Time.unscaledDeltaTime);
            return;
        }

        if (!isFullMapViewActive)
        {
            HandleEdgePanningFromCursor();
        }

        desiredPosition = ClampCameraPosition(desiredPosition);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.unscaledDeltaTime);
    }

    private void HandleZoomInput()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;
        if (MapCursor.Instance != null && MapCursor.Instance.currentMode == MapCursor.CursorMode.InFactoryMenu) return;

        float maxAllowed = GetMaxAllowedOrthographicSize();
        bool zoomChanged = false;

        // Rueda del ratón
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetOrthographicSize -= scroll * scrollZoomStep;
            zoomChanged = true;
        }

        // Tecla '+' (KeypadPlus, Plus o '=' con Shift) -> Zoom In (reduce orthographicSize)
        bool zoomInPressed = Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus) ||
                             (Input.GetKey(KeyCode.Equals) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));

        // Tecla '-' (KeypadMinus o Minus) -> Zoom Out (aumenta orthographicSize)
        bool zoomOutPressed = Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus);

        if (zoomInPressed)
        {
            targetOrthographicSize -= keyZoomSpeed * Time.unscaledDeltaTime;
            zoomChanged = true;
        }
        else if (zoomOutPressed)
        {
            targetOrthographicSize += keyZoomSpeed * Time.unscaledDeltaTime;
            zoomChanged = true;
        }

        if (zoomChanged)
        {
            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthographicSize, maxAllowed);
            isFullMapViewActive = Mathf.Abs(targetOrthographicSize - maxAllowed) < 0.05f;
        }
    }

    private void HandleToggleFullMapView()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;
        if (MapCursor.Instance != null && MapCursor.Instance.currentMode == MapCursor.CursorMode.InFactoryMenu) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            float maxAllowed = GetMaxAllowedOrthographicSize();
            isFullMapViewActive = !isFullMapViewActive;

            if (isFullMapViewActive)
            {
                positionBeforeFullMapView = desiredPosition;
                hasSavedPositionBeforeFullMapView = true;

                targetOrthographicSize = maxAllowed;
                Vector3 center = GetMapCenterWorldPosition();
                desiredPosition = ClampCameraPosition(new Vector3(center.x, center.y, transform.position.z));
            }
            else
            {
                targetOrthographicSize = Mathf.Clamp(initialOrthographicSize, minOrthographicSize, maxAllowed);

                if (TurnManager.Instance != null && TurnManager.Instance.currentActiveUnit != null)
                {
                    FocusOnUnit(TurnManager.Instance.currentActiveUnit);
                }
                else if (MapCursor.Instance != null)
                {
                    FocusOnGridPosition(MapCursor.Instance.position);
                }
                else if (hasSavedPositionBeforeFullMapView)
                {
                    desiredPosition = ClampCameraPosition(positionBeforeFullMapView);
                }
            }
        }
    }

    private Vector3 GetMapCenterWorldPosition()
    {
        float centerX = ((mapMinBounds.x + mapMaxBounds.x) * 0.5f) * cellSize;
        float centerY = ((mapMinBounds.y + mapMaxBounds.y) * 0.5f) * cellSize;
        return new Vector3(centerX, centerY, transform.position.z);
    }

    public float GetMaxAllowedOrthographicSize()
    {
        float fullMapSize = GetFullMapOrthographicSize();
        if (fitFullMapAtMaxZoomOut)
        {
            return Mathf.Max(minOrthographicSize, fullMapSize);
        }
        return Mathf.Max(minOrthographicSize, Mathf.Min(manualMaxOrthographicSize, fullMapSize));
    }

    private float GetFullMapOrthographicSize()
    {
        float mapWidthWorld = ((mapMaxBounds.x - mapMinBounds.x) + 1) * cellSize;
        float mapHeightWorld = ((mapMaxBounds.y - mapMinBounds.y) + 1) * cellSize;

        float safeAspect = (cam != null && cam.aspect > 0.01f) ? cam.aspect : (16f / 9f);
        float sizeForHeight = mapHeightWorld * 0.5f;
        float sizeForWidth = (mapWidthWorld * 0.5f) / safeAspect;

        return Mathf.Max(sizeForHeight, sizeForWidth);
    }

    private void HandleEdgePanningFromCursor()
    {
        if (MapCursor.Instance == null) return;
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;
        if (MapCursor.Instance.currentMode == MapCursor.CursorMode.InFactoryMenu) return;

        float cursorWorldX = MapCursor.Instance.position.x * cellSize;
        float cursorWorldY = MapCursor.Instance.position.y * cellSize;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float triggerDistanceWorld = edgeTriggerTiles * cellSize;

        float leftEdge = desiredPosition.x - halfWidth;
        float rightEdge = desiredPosition.x + halfWidth;
        float bottomEdge = desiredPosition.y - halfHeight;
        float topEdge = desiredPosition.y + halfHeight;

        Vector3 panDir = Vector3.zero;

        if (cursorWorldX - leftEdge <= triggerDistanceWorld) panDir.x -= 1f;
        else if (rightEdge - cursorWorldX <= triggerDistanceWorld) panDir.x += 1f;

        if (cursorWorldY - bottomEdge <= triggerDistanceWorld) panDir.y -= 1f;
        else if (topEdge - cursorWorldY <= triggerDistanceWorld) panDir.y += 1f;

        if (panDir != Vector3.zero)
        {
            desiredPosition += panDir.normalized * edgePanSpeed * Time.unscaledDeltaTime;
        }
    }

    public void FocusOnUnit(Unit unit, bool instant = false)
    {
        if (unit == null) return;
        FocusOnGridPosition(unit.position, instant);
    }

    public void FocusOnGridPosition(Vector2Int gridPos, bool instant = false)
    {
        Vector3 target = new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, transform.position.z);
        desiredPosition = ClampCameraPosition(target);

        if (instant)
        {
            transform.position = desiredPosition;
        }
    }

    public void FollowUnit(Unit unit)
    {
        unitToFollow = unit;
        isFollowingUnit = unit != null;
    }

    public void StopFollowingUnit()
    {
        isFollowingUnit = false;
        unitToFollow = null;
    }

    public void SetMapBounds(Vector2Int minBounds, Vector2Int maxBounds)
    {
        mapMinBounds = minBounds;
        mapMaxBounds = maxBounds;

        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthographicSize, GetMaxAllowedOrthographicSize());
        cam.orthographicSize = targetOrthographicSize;

        desiredPosition = ClampCameraPosition(desiredPosition);
        transform.position = desiredPosition;
    }

    private Vector3 ClampCameraPosition(Vector3 targetPos)
    {
        if (cam == null || !cam.orthographic) return targetPos;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float halfCell = cellSize * 0.5f;
        float minWorldX = (mapMinBounds.x * cellSize) - halfCell;
        float maxWorldX = (mapMaxBounds.x * cellSize) + halfCell;
        float minWorldY = (mapMinBounds.y * cellSize) - halfCell;
        float maxWorldY = (mapMaxBounds.y * cellSize) + halfCell;

        float minCamX = minWorldX + halfWidth;
        float maxCamX = maxWorldX - halfWidth;
        float minCamY = minWorldY + halfHeight;
        float maxCamY = maxWorldY - halfHeight;

        float clampedX = (minCamX > maxCamX) ? (minWorldX + maxWorldX) * 0.5f : Mathf.Clamp(targetPos.x, minCamX, maxCamX);
        float clampedY = (minCamY > maxCamY) ? (minWorldY + maxWorldY) * 0.5f : Mathf.Clamp(targetPos.y, minCamY, maxCamY);

        return new Vector3(clampedX, clampedY, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        float halfCell = cellSize * 0.5f;
        Vector3 bottomLeft = new Vector3((mapMinBounds.x * cellSize) - halfCell, (mapMinBounds.y * cellSize) - halfCell, 0f);
        Vector3 topRight = new Vector3((mapMaxBounds.x * cellSize) + halfCell, (mapMaxBounds.y * cellSize) + halfCell, 0f);

        Vector3 center = (bottomLeft + topRight) * 0.5f;
        Vector3 size = new Vector3(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}