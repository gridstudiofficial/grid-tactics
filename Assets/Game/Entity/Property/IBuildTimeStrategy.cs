using System.Collections.Generic;
using Game.Entity.Unit;

namespace Game.Entity.Property
{
	public interface IBuildTimeStrategy { int GetBuildTime(UnitData def); }

	public sealed class DefaultBuildTimeStrategy : IBuildTimeStrategy
	{
		public int GetBuildTime(UnitData def) => def.buildTime;
	}

	/*
	public sealed class OverrideBuildTimeStrategy : IBuildTimeStrategy
	{
		private readonly Dictionary<string, int> _overrides;
		private readonly IBuildTimeStrategy _fallback;
		public OverrideBuildTimeStrategy(Dictionary<string, int> overrides, IBuildTimeStrategy? fallback = null)
		{ _overrides = overrides; _fallback = fallback ?? new DefaultBuildTimeStrategy(); }
		public int GetBuildTime(UnitData def) => _overrides.TryGetValue(def.Id, out var t) ? t : _fallback.GetBuildTime(def);
	}*/
}