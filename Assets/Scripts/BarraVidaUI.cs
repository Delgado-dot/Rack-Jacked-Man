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
        if (barraVida == null)
        {
            Debug.LogWarning("[BarraVidaUI] No hay Slider asignado.");
            return;
        }

        barraVida.maxValue = PlayerHealth.GetMaxLives();
        barraVida.value = PlayerHealth.GetCurrentLives();
    }

    private void ActualizarBarra(int vidasActuales, int vidasMaximas)
    {
        if (barraVida == null) return;

        barraVida.maxValue = vidasMaximas;
        barraVida.value = vidasActuales;
    }
}
