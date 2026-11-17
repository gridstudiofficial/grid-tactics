using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Pausa { public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private GameObject botonPausa;
    public static MenuPausa Instance { get; private set; }
        public bool estaPausado = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
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
}
