using UnityEngine;
using UnityEngine.UI;

public class BarraVidaUI : MonoBehaviour
{
    public Slider barraVida;

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += ActualizarBarra;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= ActualizarBarra;
    }

    private void Start()
    {
        barraVida.maxValue = PlayerHealth.GetMaxLives();
        barraVida.value = PlayerHealth.GetCurrentLives();
    }

    private void ActualizarBarra(int vidasActuales, int vidasMaximas)
    {
        barraVida.maxValue = vidasMaximas;
        barraVida.value = vidasActuales;
    }
}