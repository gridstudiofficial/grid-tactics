
using UnityEngine;
namespace Game.Team.Player
{
    [System.Serializable]
    public class Player : MonoBehaviour ,IOwner
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [SerializeField] private int points = 0; // Puntos/Fondos del jugador

        [Header("Identidad Visual")]
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color fatigueColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        public string Id => id;
        public string Name => name;
        public int Points
        {
            get => points;
            set => points = value;
        }

        public Color BaseColor
        {
            get => baseColor;
            set => baseColor = value;
        }

        public Color FatigueColor
        {
            get => fatigueColor;
            set => fatigueColor = value;
        }

        public Player(string id, string name, Color baseColor, Color fatigueColor)
        {
            this.id = id;
            this.name = name;
            this.points = 0;
            this.baseColor = baseColor;
            this.fatigueColor = fatigueColor;
        }
        public Color GetColor(bool isFatigued)
        {
            return isFatigued ? fatigueColor : baseColor;
        }
    }
}