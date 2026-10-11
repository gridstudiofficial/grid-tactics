using UnityEngine;
using System.Collections.Generic;
using Game.Team.Player;
public abstract class Unit : MonoBehaviour, IHealable, IPositioned, ISchedulable
{
    [Header("Atributos de la Unidad")]
    [field: SerializeField] public Vector2Int position { get; set; } = Vector2Int.zero;
    [field: SerializeField] public int movementRange { get; set; } = 3;
    [field: SerializeField]  public string id { get; set; } = "";
    [Header("Visuales del Pathing")]
    [SerializeField] private GameObject rangeHighlightPrefab;
    [SerializeField] private GameObject pathHighlightPrefab;
    [field: SerializeField]  public  Player owner {get; set;}
    [field :SerializeField]public int fatigue { get; set; }
    public int currentHP { get; set; } = 10;
    public int maxHP { get; set; } = 10;

    [field: SerializeField] public bool isMyTurn { get; private set; } = false;

    private List<GameObject> activeRangeVisuals = new List<GameObject>();
    private List<GameObject> activePathVisuals = new List<GameObject>();

    [Header("Estado de Selección")]
    [field: SerializeField] private bool isSelected = false;
    public bool hasActedThisRound = false;
    private List<Vector2Int> validMoves = new List<Vector2Int>();

    private Vector2Int currentTargetGridPos; // Posición actual del cursor/destino en la cuadrícula
    private Vector3 lastMouseScreenPosition; // Para detectar movimiento físico del ratón
    private bool usingKeyboard = false;
    private Vector2Int lastRenderedTarget = new Vector2Int(-999, -999);

    private bool justTurnedOn = false;

    public Unit() => id = "";

    //public Vector2Int Position()
    //turn this.position

    //public GetAviableActions(GameState A?) { return this.Actions}

    public void TakeDamage(int damage) { this.currentHP -= damage; } //death(); }
    public void Heal(int heal) { this.currentHP = this.currentHP + heal > this.maxHP ? this.maxHP : this.currentHP + heal; }
    public void ReduceFatigue(int amount) { this.fatigue -= amount; if (this.fatigue == 0) { TurnManager.Instance.unitReady(); } }



    //public T? GetComponent<T>()

    //public T? RemoveComponent<T>()

    //protected void OnActivationStart(){}

    //protected void OnActivationEnd(){}


    protected virtual void Start()
    {
        UpdateVisualPosition();
        if (TurnManager.Instance != null) TurnManager.Instance.RegisterUnit(this);
    }

    protected virtual void Update()
    {
        if (isSelected && isMyTurn)
        {
            if (justTurnedOn)
            {
                justTurnedOn = false;
                return;
            }

            ProcessInputMode();
            UpdatePathPreview();
            HandleMovementInput();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectUnit();
            }
        }

    }

    public void StartTurn()
    {
        isMyTurn = true;
        justTurnedOn = true;
        // Opcional: Auto-seleccionar la unidad y centrar la cámara cuando inicie su turno
        SelectUnit();
    }

    public void EndTurn(int cost)
    {
        isMyTurn = false;
        DeselectUnit();
        // Notificar al gestor que terminamos y cuánta fatiga generó la acción
        TurnManager.Instance.CompleteUnitAction(this, cost);
    }

    private void OnMouseDown()
    {
        if (!isSelected && isMyTurn)
        {
            SelectUnit();
        }
    }


    private void SelectUnit()
    {
        isSelected = true;
        CalculateValidMoves();
        ShowMovementRange();

        currentTargetGridPos = position;
        lastMouseScreenPosition = Input.mousePosition;
        usingKeyboard = false;
        ShowCursor();

        Debug.Log("Unidad seleccionada. Esperando destino...");
    }

    public void DeselectUnit()
    {
        isSelected = false;
        HideMovementRange();
        ClearPathVisuals();
        ShowCursor();
        Debug.Log($"Unidad {gameObject.name} deseleccionada.");
    }

    private void ProcessInputMode()
    {
        float mouseDistance = Vector3.Distance(Input.mousePosition, lastMouseScreenPosition);
        if (mouseDistance > 0.5f)
        {
            if (usingKeyboard)
            {
                usingKeyboard = false;
                ShowCursor();
            }
            lastMouseScreenPosition = Input.mousePosition;
        }

        Vector2Int keyDirection = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) keyDirection.y += 1;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) keyDirection.y -= 1;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) keyDirection.x -= 1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) keyDirection.x += 1;

        if (keyDirection != Vector2Int.zero)
        {
            if (!usingKeyboard)
            {
                usingKeyboard = true;
                HideCursor();
            }
            currentTargetGridPos += keyDirection;
        }
        else if (!usingKeyboard)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentTargetGridPos = new Vector2Int(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));
        }
    }


    private void HandleMovementInput()
    {
        if (Input.GetMouseButtonDown(1) || (usingKeyboard && Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

            MoveTo(targetGridPos);
        }
    }

    private void MoveTo(Vector2Int targetPos)
    {
        if (!isMyTurn) return;
        Vector2Int finalDestination = GetClosestValidPosition(targetPos);
        position = finalDestination;
        int distanceMoved = Mathf.RoundToInt(Vector2.Distance(position, finalDestination));
        int totalFatigueCost = 10 + (distanceMoved * 5);


        UpdateVisualPosition();
        DeselectUnit();
        Debug.Log($"Unidad movida a {position}");

        EndTurn(totalFatigueCost);
    }


    private void CalculateValidMoves()
    {
        validMoves.Clear();
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= movementRange)
                {
                    validMoves.Add(new Vector2Int(position.x + x, position.y + y));
                }
            }
        }
    }

    private Vector2Int GetClosestValidPosition(Vector2Int clickedPos)
    {
        if (validMoves.Contains(clickedPos))
        {
            return clickedPos;
        }

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

    public void AddFatigue(int amount)
    {
        this.fatigue += amount;
    }


    // Algoritmo BFS para encontrar el camino más corto
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
                // Solo nos movemos por las casillas válidas
                if (validMoves.Contains(next) && !cameFrom.ContainsKey(next))
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        if (!cameFrom.ContainsKey(target)) return path; // No hay ruta

        Vector2Int curr = target;
        while (curr != start)
        {
            path.Add(curr);
            curr = cameFrom[curr];
        }
        path.Reverse();
        return path;
    }

    // --- REPRESENTACIÓN VISUAL ---

    private void UpdatePathPreview()
    {
        // Solo instanciar o actualizar visuales si la posición objetivo cambió
        if (currentTargetGridPos != lastRenderedTarget)
        {
            lastRenderedTarget = currentTargetGridPos;
            ClearPathVisuals();

            currentTargetGridPos = GetClosestValidPosition(currentTargetGridPos);
            List<Vector2Int> path = FindPath(position, currentTargetGridPos);

            foreach (Vector2Int pathNode in path)
            {
                if (pathHighlightPrefab != null)
                {
                    GameObject redTile = Instantiate(pathHighlightPrefab, new Vector3(pathNode.x, pathNode.y, 0), Quaternion.identity);
                    activePathVisuals.Add(redTile);
                }
            }
        }
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
        foreach (GameObject visual in activeRangeVisuals)
        {
            Destroy(visual);
        }
        activeRangeVisuals.Clear();
    }

    private void ClearPathVisuals()
    {
        foreach (GameObject visual in activePathVisuals)
        {
            Destroy(visual);
        }
        activePathVisuals.Clear();
    }



    private void UpdateVisualPosition()
    {
        transform.position = new Vector3(position.x, position.y, 0f);
    }


    private void HideCursor()
    {
        Cursor.visible = false;
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
    }

}
