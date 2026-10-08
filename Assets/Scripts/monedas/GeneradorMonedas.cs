using UnityEngine;
using TMPro; // Necesario para controlar TextMeshPro

public class GeneradorMonedas : MonoBehaviour
{
    [Header("Configuración de Economía")]
    public int totalMonedas = 0;
    public int monedasPorCiclo = 1;     // Cuántas monedas ganas cada vez
    public float tiempoPorCiclo = 1.0f; // Cada cuántos segundos ganas monedas

    [Header("Componentes de UI")]
    public TextMeshProUGUI textoContador;

    private float temporizador = 0f;

    void Start()
    {
        ActualizarTextoUI();
    }

    void Update()
    {
        // Acumula el tiempo que pasa en cada frame
        temporizador += Time.deltaTime;

        // Si el temporizador supera el tiempo del ciclo, da las monedas
        if (temporizador >= tiempoPorCiclo)
        {
            GenerarMonedasPasivas();
            temporizador = 0f; // Reinicia el temporizador
        }
    }

    void GenerarMonedasPasivas()
    {
        totalMonedas += monedasPorCiclo;
        ActualizarTextoUI();
    }

    void ActualizarTextoUI()
    {
        if (textoContador != null)
        {
            textoContador.text = totalMonedas.ToString();
        }
    }
}

