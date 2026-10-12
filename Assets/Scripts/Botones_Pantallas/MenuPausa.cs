using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private GameObject botonPausa;
    private bool estaPausado = false;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }
    public void PausarJuego()
    {
        menuPausa.SetActive(true);
        botonPausa.SetActive(false);
        Time.timeScale = 0f; // Pausa el tiempo del juego
        estaPausado = true;
    }
    public void ReanudarJuego()
    {
        menuPausa.SetActive(false);
        botonPausa.SetActive(true);
        Time.timeScale = 1f; // Reanuda el tiempo del juego
        estaPausado = false;
    }

    public void Salir(string nombreEscena)
    {
        Time.timeScale = 1f; // Asegúrate de reanudar el tiempo antes de salir
        SceneManager.LoadScene(nombreEscena);
    }
}
