using System.Collections.Generic;

namespace Game.Team
{
	public class Team
	{
		private string id { get; }
		private string name { get; }
		private List<Player.Player> members { get; }

		public Team(string id, string name)
		{
			this.id = id;
			this.name = name;
			this.members = new List<Player.Player>();
		}
	}
}