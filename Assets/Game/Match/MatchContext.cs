using Game.Data;

namespace Game.Match
{
	public static class MatchContext
	{
		// Guardamos los datos antes de hacer SceneManager.LoadScene("MatchScene")
		public static MatchSaveData CurrentMatchData { get; private set; }

		public static void SetData(MatchSaveData data)
		{
			CurrentMatchData = data;
		}
	}
}