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

        if (barraVida != null)
        {
            barraVida.maxValue = vidaMaxima;
            barraVida.value = vidaMaxima;
        }
        if (barraDanio != null)
        {
            barraDanio.maxValue = vidaMaxima;
            barraDanio.value = vidaMaxima;
        }
    }

    void Update()
    {
        if (barraDanio != null && barraVida != null && barraDanio.value > barraVida.value)
        {
            barraDanio.value -= velocidad * Time.deltaTime;
        }
    }

    public void RecibirDanio(float cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual < 0)
            vidaActual = 0;

        if (barraVida != null)
            barraVida.value = vidaActual;
    }

    public void Curar(float cantidad)
    {
        vidaActual += cantidad;

        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        if (barraVida != null)
            barraVida.value = vidaActual;
        if (barraDanio != null)
            barraDanio.value = vidaActual;
    }
}