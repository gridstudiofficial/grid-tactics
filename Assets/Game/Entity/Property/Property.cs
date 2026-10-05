using System.Collections.Generic;
using Game.Entity.Property.Component;
using Game.Team.Player;

namespace Game.Entity.Property
{
	public class Property: Entity, ISchedulable, ICapturable
	{
		public string id { get; }
		public Player owner { get; }
		public int captureProgress { get; }
		public int maxCaptureProgress { get; }
		public void ReduceCaptureProgress(int amount, Player capturer)
		{
			throw new System.NotImplementedException();
		}

		public void CompleteCapture(Player newOwner)
		{
			throw new System.NotImplementedException();
		}

		public int fatigue { get; }

		protected List<IPropertyComponent> components = new List<IPropertyComponent>();

	}
}