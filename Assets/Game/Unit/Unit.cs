using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Team.Player;
using TMPro;

public abstract class Unit : MonoBehaviour, IHealable, IPositioned, ISchedulable
{
    [Header("Atributos de la Unidad")]
    [field: SerializeField] public Vector2Int position { get; set; } = Vector2Int.zero;
    [field: SerializeField] public int movementRange { get; set; } = 3;
    [field: SerializeField] public string id { get; set; } = "";
    [SerializeField] private float moveSpeed = 5f;
    public bool canFly = false; // Define si ignora restricciones de terreno

    [Header("Visuales y UI Local")]
    [SerializeField] private GameObject rangeHighlightPrefab;
    [SerializeField] private GameObject pathHighlightPrefab;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;
    [SerializeField] private TextMeshProUGUI hpTextDisplay; // Texto con la vida al lado de la ficha

    [Header("Colores de Estado")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color exhaustedColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Oscuro al agotar turno

    [field: SerializeField] public Player owner { get; set; }
    [field: SerializeField] public int fatigue { get; set; }
    public int currentHP { get; set; } = 10;
    public int maxHP { get; set; } = 10;

    [field: SerializeField] public bool isMyTurn { get; private set; } = false;
    public bool hasActedThisRound = false;

    private List<GameObject> activeRangeVisuals = new List<GameObject>();
    private List<GameObject> activePathVisuals = new List<GameObject>();
    private List<Vector2Int> validMoves = new List<Vector2Int>();

    public List<UnitAction> availableActions = new List<UnitAction>();

    private bool isMoving = false;
    private int pendingFatigueCost = 0;
    private Vector2Int initialPosition;
    private int initialFatigue;

    [Header("Clasificación y Combate")]
    [field: SerializeField] public int unitID { get; set; } = 0; // ID numérico para la matriz
    [field: SerializeField] public UnitMovementType movementType { get; set; } = UnitMovementType.Ground;

    public void TakeDamage(int damage)
    {
        this.currentHP = Mathf.Max(0, this.currentHP - damage);
        UpdateHealthDisplay();
    }

    public void Heal(int heal)
    {
        this.currentHP = Mathf.Min(this.currentHP + heal, this.maxHP);
        UpdateHealthDisplay();
    }

    public void ReduceFatigue(int amount)
    {
        this.fatigue -= amount;
        if (this.fatigue <= 0) { TurnManager.Instance.unitReady(); }
    }
    public void AddFatigue(int amount) { this.fatigue += amount; }

    protected virtual void Awake()
    {
        activeColor = this.GetComponent<SpriteRenderer>().color;
        exhaustedColor = new Color(activeColor.r * 0.4f, activeColor.g * 0.4f, activeColor.b * 0.4f, 1f);
    }

    protected virtual void Start()
    {
        if (unitSpriteRenderer == null) unitSpriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisualPosition();
        UpdateHealthDisplay();
        SetExhaustedState(false); // Color normal al inicio

        if (TurnManager.Instance != null) TurnManager.Instance.RegisterUnit(this);
    }

    public void UpdateHealthDisplay()
    {
        if (hpTextDisplay != null)
        {
            hpTextDisplay.text = currentHP.ToString();
        }
    }

    public void SetExhaustedState(bool exhausted)
    {
        if (unitSpriteRenderer != null)
        {
            unitSpriteRenderer.color = exhausted ? exhaustedColor : activeColor;
        }
    }

    public void StartTurn()
    {
        isMyTurn = true;
        SetExhaustedState(false);
    }

    public void BeginMovementSelection()
    {
        initialPosition = position;
        initialFatigue = fatigue;
        CalculateValidMovesWithTerrain();
        ShowMovementRange();
    }

    public List<Vector2Int> GetValidMoves() => validMoves;

    public void ExecuteMovementTo(Vector2Int targetPos)
    {
        if (!isMyTurn || isMoving) return;

        List<Vector2Int> path = FindPathTo(targetPos);
        if (path.Count > 0)
        {
            pendingFatigueCost = 10 + (path.Count * 5);
            HideMovementRange();
            ClearPathVisuals();
            StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            pendingFatigueCost = 10;
            HideMovementRange();
            ClearPathVisuals();
            OpenActionMenu();
        }
    }

    private IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isMoving = true;
        foreach (Vector2Int step in path)
        {
            Vector3 targetWorldPos = new Vector3(step.x, step.y, 0f);
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetWorldPos;
            position = step;
        }
        isMoving = false;
        OpenActionMenu();
    }

    private void OpenActionMenu()
    {
        MapCursor.Instance.SetActionMenuMode(this);
        if (ActionMenuUI.Instance != null)
        {
            ActionMenuUI.Instance.ShowMenu(this);
        }
        else
        {
            ExecuteWaitAction();
        }
    }

    public void ExecuteWaitAction()
    {
        EndTurn(pendingFatigueCost);
    }

    public void EndTurn(int cost)
    {
        isMyTurn = false;
        SetExhaustedState(true); // Oscurecer el sprite al acabar la acción
        DeselectUnit();
        MapCursor.Instance.ResetToFreeMode();
        TurnManager.Instance.CompleteUnitAction(this, cost);
    }

    public void RevertToInitialState()
    {
        position = initialPosition;
        fatigue = initialFatigue;
        SetExhaustedState(false);
        UpdateVisualPosition();
        DeselectUnit();
    }

    public void DeselectUnit()
    {
        HideMovementRange();
        ClearPathVisuals();
    }

    // --- CÁLCULO DE MOVIMIENTO RECONOCIENDO TERRENO ---
    private void CalculateValidMovesWithTerrain()
    {
        validMoves.Clear();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(position);
        costSoFar[position] = 0;
        validMoves.Add(position);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                // Verificar si el tipo de unidad puede pasar por este terreno
                TerrainType terrain = MapTerrain.Instance != null ? MapTerrain.Instance.GetTerrainAt(next) : TerrainType.Plains;
                bool canPass = MapTerrain.Instance == null || MapTerrain.Instance.CanUnitTypePassTerrain(movementType, terrain);

                if (canPass)
                {
                    int newCost = costSoFar[current] + 1; // Coste 1 por casilla transitable

                    if (newCost <= movementRange)
                    {
                        if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                        {
                            costSoFar[next] = newCost;
                            frontier.Enqueue(next);
                            if (!validMoves.Contains(next)) validMoves.Add(next);
                        }
                    }
                }
            }
        }
    }

    public Vector2Int GetClosestValidPosition(Vector2Int clickedPos)
    {
        if (validMoves.Contains(clickedPos)) return clickedPos;

        Vector2Int bestTile = position;
        float minDistance = float.MaxValue;

        foreach (Vector2Int tile in validMoves)
        {
            float dist = Vector2.Distance(tile, clickedPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestTile = tile;
            }
        }
        return bestTile;
    }

    public List<Vector2Int> FindPathTo(Vector2Int target)
    {
        Vector2Int validTarget = GetClosestValidPosition(target);
        return FindPath(position, validTarget);
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == target) break;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (validMoves.Contains(next) && !cameFrom.ContainsKey(next))
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        if (!cameFrom.ContainsKey(target)) return path;

        Vector2Int curr = target;
        while (curr != start)
        {
            path.Add(curr);
            curr = cameFrom[curr];
        }
        path.Reverse();
        return path;
    }

    protected virtual void ShowMovementRange()
    {
        if (rangeHighlightPrefab == null) return;
        foreach (Vector2Int tile in validMoves)
        {
            GameObject blueTile = Instantiate(rangeHighlightPrefab, new Vector3(tile.x, tile.y, 0), Quaternion.identity);
            activeRangeVisuals.Add(blueTile);
        }
    }

    protected virtual void HideMovementRange()
    {
        foreach (GameObject visual in activeRangeVisuals) Destroy(visual);
        activeRangeVisuals.Clear();
    }

    private void ClearPathVisuals()
    {
        foreach (GameObject visual in activePathVisuals) Destroy(visual);
        activePathVisuals.Clear();
    }

    private void UpdateVisualPosition()
    {
        transform.position = new Vector3(position.x, position.y, 0f);
    }
}