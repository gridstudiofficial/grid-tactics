using System;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para gestionar escenas

public class CambioEscena : MonoBehaviour
{
    public void CargarEscenaPorNombre(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    public void CargarSelectorNiveles()
    {
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Console.WriteLine("Saliendo del juego");
        Application.Quit();
    }
}
