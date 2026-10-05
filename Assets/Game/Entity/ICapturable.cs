
using Game.Team.Player;

namespace Game.Entity
{
	public interface ICapturable
	{

		Player owner { get; }
		int captureProgress { get; }
		int maxCaptureProgress { get; }
		void ReduceCaptureProgress(int amount, Player capturer);
		void CompleteCapture(Player newOwner);
	}
}