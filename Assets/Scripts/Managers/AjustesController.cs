using UnityEngine;
using TMPro;

public class AjustesController : MonoBehaviour
{
    [SerializeField] private TMP_Text textoSonido;

    private bool silenciado = false;

    private void OnEnable()
    {
        ActualizarTexto();
    }

    public void Cerrar()
    {
        gameObject.SetActive(false);
    }

    public void AlternarSonido()
    {
        silenciado = !silenciado;
        AudioListener.volume = silenciado ? 0f : 1f;
        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (textoSonido != null)
            textoSonido.text = silenciado ? "Sonido: OFF" : "Sonido: ON";
    }
}
