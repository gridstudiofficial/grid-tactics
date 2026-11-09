
using UnityEngine;
namespace Game.Team.Player
{
    [System.Serializable]
    public class Player : MonoBehaviour ,IOwner
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [SerializeField] private int points = 0; // Puntos/Fondos del jugador

        public string Id => id;
        public string Name => name;
        public int Points
        {
            get => points;
            set => points = value;
        }

        public Player(string id, string name)
        {
            this.id = id;
            this.name = name;
            this.points = 0;
        }
    }
}