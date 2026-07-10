using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinoNivel : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private string siguienteNivel = "";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Destino alcanzado. Cargando siguiente nivel...");

        if (!string.IsNullOrEmpty(siguienteNivel))
        {
            SceneManager.LoadScene(siguienteNivel);
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.Log("No hay siguiente nivel configurado.");
            }
        }
    }
}
