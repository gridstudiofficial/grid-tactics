using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para gestionar escenas

public class CambioEscena : MonoBehaviour
{
    public void CargarEscenaPorNombre(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}
