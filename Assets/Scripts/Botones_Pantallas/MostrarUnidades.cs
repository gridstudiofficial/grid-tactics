using System;
using UnityEngine;

public class MostrarUnidades : MonoBehaviour
{
    [SerializeField] private GameObject menuUnidades;
    [SerializeField] private GameObject botonHide;
    [SerializeField] private GameObject botonShow;
    public void Mostrar()
    {
        menuUnidades.SetActive(true);
        botonHide.SetActive(true);
        botonShow.SetActive(false);
    }
    public void Esconder()
    {
        Debug.Log("tu puta madre");
        menuUnidades.SetActive(false);
        botonHide.SetActive(false);
        botonShow.SetActive(true);
    }
}
