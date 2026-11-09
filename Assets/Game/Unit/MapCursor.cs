using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapCursor : MonoBehaviour
{
    public static MapCursor Instance { get; private set; }

    [Header("Configuración de Movimiento")]
    public Vector2Int position = Vector2Int.zero;
    public float moveCooldown = 0.15f;
    private float nextMoveTime = 0f;

    [Header("Visuales del Pathing")]
    [SerializeField] private GameObject pathHighlightPrefab;
    [SerializeField] private GameObject attackReticlePrefab;
    private List<GameObject> activePathVisuals = new List<GameObject>();
    private GameObject activeReticleInstance;

    [Header("Estados del Cursor")]
    private Unit activeUnit = null;
    private CursorMode currentMode = CursorMode.FreeExploration;
    private AttackAction currentAttackAction;

    private List<Unit> currentEnemiesInRange = new List<Unit>();
    private int selectedEnemyIndex = 0;

    public enum CursorMode
    {
        FreeExploration,
        SelectingMovement,
        SelectingAction,
        SelectingAttack,
        InFactoryMenu // Estado activo al interactuar con la fábrica
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        UpdateVisualPosition();

        switch (currentMode)
        {
            case CursorMode.FreeExploration:
                HandleFreeMovement();
                // Al pulsar F en exploración libre, intenta seleccionar unidad o abrir fábrica
                if (Input.GetKeyDown(KeyCode.F)) TryInteractTile();
                break;

            case CursorMode.SelectingMovement:
                HandleFreeMovement();
                UpdatePathPreview();
                if (Input.GetKeyDown(KeyCode.F)) ConfirmMovement();
                if (Input.GetKeyDown(KeyCode.Escape)) CancelAndRevertActiveUnit();
                break;

            case CursorMode.SelectingAction:
                if (Input.GetKeyDown(KeyCode.Escape)) CancelAndRevertActiveUnit();
                break;

            case CursorMode.SelectingAttack:
                HandleAttackEnemyCycling();
                if (Input.GetKeyDown(KeyCode.F)) ConfirmAttackTarget();
                if (Input.GetKeyDown(KeyCode.Escape)) CancelAttackTargeting();
                break;

            case CursorMode.InFactoryMenu:
                // Si la UI de fábrica maneja su propia navegación por teclado, 
                // solo escuchamos Escape para cerrar el menú
                if (Input.GetKeyDown(KeyCode.Escape)) CloseFactoryMenu();
                break;
        }
    }

    private void HandleFreeMovement()
    {
        Vector2Int dir = GetInputDirection();
        if (dir != Vector2Int.zero && Time.time >= nextMoveTime)
        {
            position += dir;
            nextMoveTime = Time.time + moveCooldown;
        }
    }

    private Vector2Int GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) return Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) return Vector2Int.right;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return Vector2Int.down;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return Vector2Int.right;

        return Vector2Int.zero;
    }

    private void UpdateVisualPosition()
    {
        transform.position = new Vector3(position.x, position.y, 0f);
    }

    // --- INTERACCIÓN DESDE EXPLORACIÓN LIBRE (UNIDADES O FÁBRICAS) ---

    private void TryInteractTile()
    {
        if (TurnManager.Instance == null) return;

        // 1. Intentar seleccionar una unidad del jugador activo que no haya actuado
        Unit unitUnderCursor = TurnManager.Instance.allUnits.FirstOrDefault(u =>
            u.position == position &&
            u.owner == TurnManager.Instance.activePlayer &&
            u.fatigue <= 0 &&
            !u.hasActedThisRound);

        if (unitUnderCursor != null && unitUnderCursor.isMyTurn)
        {
            activeUnit = unitUnderCursor;
            currentMode = CursorMode.SelectingMovement;
            activeUnit.BeginMovementSelection();
            return;
        }

        if(unitUnderCursor != null)
        {
            return;
        }

        // 2. Si no hay unidad, buscar si hay una Fábrica en esta casilla que pertenezca al jugador activo
        Factory factoryUnderCursor = Object.FindObjectsOfType<Factory>().FirstOrDefault(f =>
            f.position == position &&
            f.owner == TurnManager.Instance.activePlayer);

        if (factoryUnderCursor != null)
        {
            OpenFactoryMenu(factoryUnderCursor);
        }
    }

    private void OpenFactoryMenu(Factory factory)
    {
        currentMode = CursorMode.InFactoryMenu;

        if (FactoryMenuUI.Instance != null)
        {
            FactoryMenuUI.Instance.ShowFactoryMenu(factory);
            Debug.Log($"Abriendo menú de fábrica en {factory.position} para {factory.owner.Name}");
        }
        else
        {
            Debug.LogWarning("FactoryMenuUI no está asignado o presente en la escena.");
        }
    }

    public void CloseFactoryMenu()
    {
        if (FactoryMenuUI.Instance != null)
        {
            FactoryMenuUI.Instance.HideFactoryMenu();
        }
        currentMode = CursorMode.FreeExploration;
    }

    // --- GESTIÓN DE PATHING Y MOVIMIENTO ---

    private void UpdatePathPreview()
    {
        ClearPathVisuals();
        if (activeUnit == null) return;

        Vector2Int validTarget = activeUnit.GetClosestValidPosition(position);
        List<Vector2Int> path = activeUnit.FindPathTo(validTarget);

        foreach (var node in path)
        {
            if (pathHighlightPrefab != null)
            {
                GameObject redTile = Instantiate(pathHighlightPrefab, new Vector3(node.x, node.y, 0), Quaternion.identity);
                activePathVisuals.Add(redTile);
            }
        }
    }

    private void ClearPathVisuals()
    {
        foreach (var visual in activePathVisuals) Destroy(visual);
        activePathVisuals.Clear();
    }

    private void ConfirmMovement()
    {
        if (activeUnit == null) return;

        if (activeUnit.GetValidMoves().Contains(position))
        {
            ClearPathVisuals();
            activeUnit.ExecuteMovementTo(position);
        }
        else
        {
            Debug.Log("Casilla fuera del rango de movimiento permitido.");
        }
    }

    public void SetActionMenuMode(Unit unit)
    {
        activeUnit = unit;
        currentMode = CursorMode.SelectingAction;
        ClearPathVisuals();
    }

    // --- GESTIÓN DE ATAQUE Y MIRA ---

    public void StartAttackTargeting(AttackAction action)
    {
        currentAttackAction = action;
        currentMode = CursorMode.SelectingAttack;

        currentEnemiesInRange = action.FindEnemiesInRange(activeUnit);
        selectedEnemyIndex = 0;

        if (currentEnemiesInRange.Count > 0)
        {
            position = currentEnemiesInRange[0].position;
            ShowAttackReticle(currentEnemiesInRange[0].transform.position);
        }
        else
        {
            Debug.Log("No hay objetivos enemigos en rango.");
            CancelAttackTargeting();
        }
    }

    private void HandleAttackEnemyCycling()
    {
        if (currentEnemiesInRange.Count == 0) return;

        Vector2Int dir = GetInputDirection();
        if (dir != Vector2Int.zero && Time.time >= nextMoveTime)
        {
            if (dir.x > 0 || dir.y > 0)
            {
                selectedEnemyIndex = (selectedEnemyIndex + 1) % currentEnemiesInRange.Count;
            }
            else if (dir.x < 0 || dir.y < 0)
            {
                selectedEnemyIndex = (selectedEnemyIndex - 1 + currentEnemiesInRange.Count) % currentEnemiesInRange.Count;
            }

            position = currentEnemiesInRange[selectedEnemyIndex].position;
            UpdateReticlePosition();
            nextMoveTime = Time.time + moveCooldown;
        }
    }

    private void ShowAttackReticle(Vector3 worldPos)
    {
        if (attackReticlePrefab != null && activeReticleInstance == null)
        {
            activeReticleInstance = Instantiate(attackReticlePrefab, worldPos, Quaternion.identity);
        }
    }

    private void UpdateReticlePosition()
    {
        if (activeReticleInstance != null && currentEnemiesInRange.Count > 0)
        {
            activeReticleInstance.transform.position = new Vector3(position.x, position.y, 0f);
        }
    }

    private void ConfirmAttackTarget()
    {
        Unit targetEnemy = currentEnemiesInRange.FirstOrDefault(u => u.position == position);
        if (targetEnemy != null)
        {
            DestroyReticle();
            currentAttackAction.PerformAttack(activeUnit, targetEnemy);
            currentAttackAction = null;
            currentEnemiesInRange.Clear();
            currentMode = CursorMode.FreeExploration;
            activeUnit = null;
        }
    }

    private void CancelAttackTargeting()
    {
        DestroyReticle();
        currentAttackAction = null;
        currentEnemiesInRange.Clear();
        currentMode = CursorMode.SelectingAction;
        if (ActionMenuUI.Instance != null)
        {
            ActionMenuUI.Instance.ShowMenu(activeUnit);
        }
    }

    private void DestroyReticle()
    {
        if (activeReticleInstance != null) Destroy(activeReticleInstance);
    }

    public void CancelAndRevertActiveUnit()
    {
        if (activeUnit != null)
        {
            if (ActionMenuUI.Instance != null)
            {
                ActionMenuUI.Instance.HideMenu();
            }

            activeUnit.RevertToInitialState();
            position = activeUnit.position;
            activeUnit = null;
        }
        ClearPathVisuals();
        currentMode = CursorMode.FreeExploration;
    }

    public void ResetToFreeMode()
    {
        activeUnit = null;
        ClearPathVisuals();
        currentMode = CursorMode.FreeExploration;
    }
}