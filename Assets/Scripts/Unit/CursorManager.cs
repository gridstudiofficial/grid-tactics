using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private bool isCursorHidden = false;

    void Start()
    {
        // Ocultar el cursor al iniciar
        HideCursor();
    }

    void Update()
    {
        // Detectar si el ratón se está moviendo
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (mouseX != 0f || mouseY != 0f)
        {
            ShowCursor();
        }
    }

    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None; // Cambia a Locked si prefieres centrarlo
        isCursorHidden = true;
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isCursorHidden = false;
    }
}
