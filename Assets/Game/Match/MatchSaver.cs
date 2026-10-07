using System.Collections.Generic;
using Game.Data;
using Game.Entity.Unit;
using UnityEngine;

namespace Game.Match
{
	public class MatchSaver : MonoBehaviour
	{
		public MatchSaveData CreateSaveData()
		{
			MatchSaveData saveData = new MatchSaveData();

			// Capturar estado de unidades
			saveData.units = new List<UnitData>();
			foreach (var unit in FindObjectsOfType<Unit>())
			{
				//saveData.units.Add(unit.ToData());
			}

			// Capturar estado de casillas/mapa
			// ... Recorrer matriz de mapa y generar saveData.tileMap

			return saveData;
		}

		public void SaveToFile(string filePath)
		{
			MatchSaveData data = CreateSaveData();
			string json = JsonUtility.ToJson(data, true);
			System.IO.File.WriteAllText(filePath, json);
		}
	}
}