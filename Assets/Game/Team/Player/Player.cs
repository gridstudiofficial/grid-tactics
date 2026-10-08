using UnityEngine;

namespace Game.Team.Player
{
	[System.Serializable]
	public class Player: IOwner
	{
		[SerializeField] private string id;
		[SerializeField] private string name;

		public string Id => id;
		public string Name => name;

		public Player(string id, string name)
		{
			this.id = id;
			this.name = name;
		}
	}
}