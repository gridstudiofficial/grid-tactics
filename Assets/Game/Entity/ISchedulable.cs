#nullable enable
using Game.Team.Player;

namespace Game.Entity
{
	public interface ISchedulable
	{
		string id { get; }
		Player? owner { get; } // null = neutral
		int fatigue { get; }
	}
}
