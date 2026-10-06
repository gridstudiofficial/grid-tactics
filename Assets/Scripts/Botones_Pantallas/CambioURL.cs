using UnityEngine;

public class CambioURL : MonoBehaviour
{
    public void AbrirURL(string url)
    {
        Application.OpenURL(url);
    }
}
