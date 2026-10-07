using UnityEngine;
using System.Collections.Generic;
public abstract class Unit : MonoBehaviour, IHealable, IPositioned
{
    [Header("Atributos de la Unidad")]

    [field: SerializeField] public Vector2Int position { get; set; } = Vector2Int.zero;
    [field: SerializeField] public int movementRange { get; set; } = 3;
    public string id { get; set; } = "";
    [Header("Visuales del Pathing")]
    [SerializeField] private GameObject rangeHighlightPrefab;
    [SerializeField] private GameObject pathHighlightPrefab;
    //public  Player? owner {get; set;}
    public int fatigue { get; set; }
    public int currentHP { get; set; } = 10;
    public int maxHP { get; set; } = 10;

    private List<GameObject> activeRangeVisuals = new List<GameObject>();
    private List<GameObject> activePathVisuals = new List<GameObject>();
    private Vector2Int lastMouseGridPos;

    //public UnitStatus status {get; set;}
    //public IMovementProfile movement {get; set;}
    //public List<IWeapon> Weapons {get; set;}
    [Header("Estado de Selección")]
    [field: SerializeField] private bool isSelected = false;
    private List<Vector2Int> validMoves = new List<Vector2Int>();

    private bool isCursorHidden = false;

    public Unit() => id = "";

    //public Vector2Int Position()
    //turn this.position

    //public GetAviableActions(GameState A?) { return this.Actions}

    public void TakeDamage(int damage) { this.currentHP -= damage; } //death(); }
    public void Heal(int heal) { this.currentHP = this.currentHP + heal > this.maxHP ? this.maxHP : this.currentHP + heal; }
    public void ReduceFatigue(int amount) { this.fatigue -= amount; }



    //public T? GetComponent<T>()

    //public T? RemoveComponent<T>()

    //protected void OnActivationStart(){}

    //protected void OnActivationEnd(){}


    protected virtual void Start()
    {
        UpdateVisualPosition();
    }

    protected virtual void Update()
    {


        if (isSelected)
        {

            UpdatePathPreview();
            HandleMovementInput();
            ShowCursor();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectUnit();
            }
        }

    }

    private void OnMouseDown()
    {
        if (!isSelected)
        {
            SelectUnit();
        }
    }


    private void SelectUnit()
    {
        isSelected = true;
        CalculateValidMoves();
        ShowMovementRange();
        lastMouseGridPos = new Vector2Int(-999, -999);
        Debug.Log("Unidad seleccionada. Esperando destino...");
    }

    public void DeselectUnit()
    {
        isSelected = false;
        HideMovementRange();
        ClearPathVisuals();
        Debug.Log($"Unidad {gameObject.name} deseleccionada.");
    }

    private void HandleMovementInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

            MoveTo(targetGridPos);
        }

        HideCursor();


    }

    private void MoveTo(Vector2Int targetPos)
    {
        Vector2Int finalDestination = GetClosestValidPosition(targetPos);

        // Actualizar posición lógica
        //gridPosition = finalDestination;
        position = finalDestination;

        UpdateVisualPosition();
        DeselectUnit();
        Debug.Log($"Unidad movida a {position}");
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int currentMouseGridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        // Solo recalcula si el ratón se movió de casilla
        if (currentMouseGridPos != lastMouseGridPos || Input.anyKey)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastMouseGridPos += new Vector2Int(0, 1);
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastMouseGridPos += new Vector2Int(-1, 0);
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastMouseGridPos += new Vector2Int(1, 0);
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastMouseGridPos += new Vector2Int(0, -1);
            }
            lastMouseGridPos = currentMouseGridPos;
            ClearPathVisuals();

            Vector2Int validTarget = GetClosestValidPosition(currentMouseGridPos);

            // Si apuntamos a nosotros mismos, no mostramos camino rojo
            //if (validTarget == position) return;

            List<Vector2Int> path = FindPath(position, validTarget);

            // Dibujar el camino instanciando los prefabs rojos
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
        Cursor.lockState = CursorLockMode.None; // Cambia a Locked si prefieres centrarlo
        isCursorHidden = true;
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isCursorHidden = false;
    }

}
