using System.Collections.Generic;
using Game.Entity.Property;
using Game.Entity.Unit;

namespace Game.Map
{
	public class Map
	{
		private Tile.Tile[,] map;
		private HashSet<Unit> units;
		private HashSet<Property> properties;
	}
}