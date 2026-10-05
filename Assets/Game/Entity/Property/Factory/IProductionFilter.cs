using System.Collections.Generic;
using System.Linq;
using Game.Entity.Unit;

namespace Game.Entity.Property.Factory
{
	public interface IProductionFilter
	{
		bool Allows(UnitData def);
	}

	public sealed class CategoryProductionFilter : IProductionFilter
	{
		private readonly HashSet<UnitCategory> _allowed;
		public CategoryProductionFilter(params UnitCategory[] allowed) => _allowed = allowed.ToHashSet();
		public bool Allows(UnitData def) => _allowed.Contains(def.category);
	}
	/*
	public sealed class WhitelistProductionFilter : IProductionFilter
	{
		private readonly HashSet<string> ids;
		public WhitelistProductionFilter(params string[] unitDefIds) => ids = unitDefIds.ToHashSet();
		public bool Allows(UnitData def) => ids.Contains(def.id);
	}*/
}