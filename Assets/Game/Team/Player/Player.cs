namespace Game.Team.Player
{
	public abstract class Player
	{
		public string id { get; }
		public Team team { get; }
		public string name { get; }
		public int funds { get; protected set; }
		private PlayerState state { get; set; }
		public Player(string id, string name, Team team, int funds = 0)
		{
			this.id = id;
			this.name = name;
			this.team = team;
			this.funds = funds;
		}
	}
}