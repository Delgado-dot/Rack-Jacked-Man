using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Slider barraVida;
    public Slider barraDanio;

    public float vidaMaxima = 100;
    public float vidaActual;

    public float velocidad = 50f;

    void Start()
    {
        vidaActual = vidaMaxima;

        barraVida.maxValue = vidaMaxima;
        barraDanio.maxValue = vidaMaxima;

        barraVida.value = vidaMaxima;
        barraDanio.value = vidaMaxima;
    }

    void Update()
    {
        if (barraDanio.value > barraVida.value)
        {
            barraDanio.value -= velocidad * Time.deltaTime;
        }
    }

    public void RecibirDanio(float cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual < 0)
            vidaActual = 0;

        barraVida.value = vidaActual;
    }

    public void Curar(float cantidad)
    {
        vidaActual += cantidad;

        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        barraVida.value = vidaActual;
        barraDanio.value = vidaActual;
    }
}