using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
	[Serializable]
	public class MatchSaveData
	{
		public string mapId;
		public RulesData rules;
		public List<TeamData> teams;
		public List<UnitData> units;
		public TileData[] tileMap;
		public int currentTurn;
	}

	[Serializable]
	public class TeamData
	{
		public string teamId;
		public string teamName;
		public List<string> playerIds;
	}

	[Serializable]
	public class UnitData
	{
		public string instanceId;
		public string unitTypeId;
		public string ownerTeamId;
		public Vector2Int position;
		public int currentHealth;
		public int fatigue;
	}

	[Serializable]
	public class TileData
	{
		public Vector2Int position;
		public string tileTypeId;
		public bool isOccupied;
	}

	[Serializable]
	public class RulesData
	{
		public string winConditionId;
		public int maxTurns;
	}
}