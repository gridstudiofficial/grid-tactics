using System.Collections.Generic;
using Game.Entity.Property.Factory;
using Game.Entity.Unit;

namespace Game.Entity.Property.Component
{

	public sealed class ProductionBay : IPropertyComponent
	{
		public IProductionFilter Filter { get; }
		public IBuildTimeStrategy TimeStrategy { get; set; }
		private readonly Queue<UnitData> _pending = new();

		public ProductionBay(IProductionFilter filter, IBuildTimeStrategy? timeStrategy = null)
		{ Filter = filter; TimeStrategy = timeStrategy ?? new DefaultBuildTimeStrategy(); }

		public bool CanBuild(UnitData def) => Filter.Allows(def);
		public void Enqueue(UnitData def) => _pending.Enqueue(def);
	}
}