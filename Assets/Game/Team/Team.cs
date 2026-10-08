using System.Collections.Generic;

namespace Game.Team
{
	 [System.Serializable]
	public class Team
	{
		public string name;
		public string id;
		public List<Player.Player> players;
	}
}