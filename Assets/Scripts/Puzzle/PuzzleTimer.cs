using TMPro;
using UnityEngine;

public class PuzzleTimer : MonoBehaviour
{
    public float tiempoInicial = 30f;
    public TMP_Text timerText;
    public PuzzleMana puzzleMana;

    private float tiempoActual;
    private bool contando;

    private void Update()
    {
        if (!contando)
            return;

        tiempoActual -= Time.unscaledDeltaTime;
        timerText.text = "Tiempo: " + Mathf.Ceil(tiempoActual);

        if (tiempoActual <= 0)
        {
            contando = false;
            Debug.Log("Tiempo agotado");
            puzzleMana.PuzzleFailed();
        }
    }

    public void IniciarTiempo()
    {
        tiempoActual = tiempoInicial;
        contando = true;
    }

    public void DetenerTiempo()
    {
        contando = false;
    }
}
