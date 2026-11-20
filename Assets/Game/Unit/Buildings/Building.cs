using UnityEngine;
using Game.Team.Player;

namespace Game.Unit.Builgings
{
    public abstract class Building : MonoBehaviour, IPositioned
    {
        [field: SerializeField] public Vector2Int position { get; set; } = Vector2Int.zero;
        [field: SerializeField] public Player owner { get; set; } = null; // Null si es neutral
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Header("Visuales")]
        [SerializeField] protected Color neutralColor = Color.gray;

        protected virtual void Start()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
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
                spriteRenderer.color = owner != null ? owner.BaseColor : neutralColor;
            }
        }

        private void UpdateVisualPosition()
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }
    }
}