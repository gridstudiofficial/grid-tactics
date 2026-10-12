using System;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para gestionar escenas

public class CambioEscena : MonoBehaviour
{
    public void CargarEscenaPorNombre(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir()
    {
        Console.WriteLine("Saliendo del juego");
        Application.Quit();
    }
}
