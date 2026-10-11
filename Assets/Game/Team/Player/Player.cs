using UnityEngine;

namespace Game.Team.Player
{
	[System.Serializable]
	public class Player: MonoBehaviour, IOwner
	{
		[field: SerializeField] public string id { get; set; }
		[field: SerializeField] public string nickName { get; set; }

        public Player(string id, string name)
		{
			this.id = id;
			this.nickName = name;
		}
	}
}