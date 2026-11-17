using UnityEngine;
using Game.Team.Player;
using Game.Unit.Builgings;

public class GameInitializer : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public void StartGame()
    {
        // Asignar puntos iniciales si lo deseas
        player1.Points = 2000;

        // 2. Asignar el jugador activo inicial en el TurnManager
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.activePlayer = player1;
        }

        // 3. Buscar fábricas o ciudades en la escena y asignarles un dueño inicial de prueba
        Building[] buildings = FindObjectsOfType<Building>();
        foreach (var building in buildings)
        {
            building.SetOwner(player1); // Asignamos la base al Jugador 1 para pruebas
        }
        TurnManager.Instance.StartNextBatchOrReduce();
    }
}