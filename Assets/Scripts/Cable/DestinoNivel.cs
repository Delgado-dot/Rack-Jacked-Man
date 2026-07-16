using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinoNivel : MonoBehaviour
{
    [SerializeField] private string siguienteNivel = "";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[DestinoNivel] Nivel completado.");

        // Si se configuró un destino explícito, usarlo
        if (!string.IsNullOrEmpty(siguienteNivel))
        {
            SceneManager.LoadScene(siguienteNivel);
            return;
        }

        // Si hay GameManager, usar su lógica de destino según nivel actual
        if (GameManager.Instance != null)
        {
            string destino = GameManager.Instance.GetSubLevelDestination();
            Debug.Log("[DestinoNivel] GameManager destino: " + destino);
            GameManager.Instance.AvanzarNivel();
            SceneManager.LoadScene(destino);
            return;
        }

        // Fallback: siguiente build index
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }
}
