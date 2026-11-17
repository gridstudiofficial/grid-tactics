using UnityEngine;
using Game.Team.Player;

namespace Game.Unit.Builgings
{public abstract class Building : MonoBehaviour, IPositioned
{
    [field: SerializeField] public Vector2Int position { get; set; } = Vector2Int.zero;
    [field: SerializeField] public Player owner { get; set; } = null; // Null si es neutral
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Visuales")]
    [SerializeField] private Color neutralColor = Color.gray;

    protected virtual void Start()
    {
        UpdateVisualPosition();
        UpdateVisuals();
    }

    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        UpdateVisuals();
    }

    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            // Aquí puedes cambiar el color según el jugador o mantener un color base
            spriteRenderer.color = owner != null ? Color.cyan : neutralColor;
        }
    }

    private void UpdateVisualPosition()
    {
        transform.position = new Vector3(position.x, position.y, 0f);
    }
}}