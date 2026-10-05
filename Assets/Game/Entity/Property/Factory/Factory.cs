using Game.Entity.Property.Component;

namespace Game.Entity.Property.Factory
{
	public class Factory : Property
	{
		public Factory()
		{
			this.components.Add(new ProductionBay(new CategoryProductionFilter()));
		}
	}
}