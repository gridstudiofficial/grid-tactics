using Game.Team;
using Game.Team.Player;

namespace Game.Entity
{

	public interface ISchedulable
	{
		string Id { get; }
		IOwner Owner { get; }
		int Fatigue { get; }
	}
}