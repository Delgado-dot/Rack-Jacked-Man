using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinoNivel : MonoBehaviour
{
    [SerializeField] private string siguienteNivel = "";

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (!other.CompareTag("Player")) return;

        activado = true;

        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("[DIAG-DESTINO] ═══ OnTriggerEnter en \"" + gameObject.name + "\" | Escena actual: " + escenaActual + " | siguienteNivel=\"" + siguienteNivel + "\"");

        // Si se configuró un destino explícito, usarlo
        if (!string.IsNullOrEmpty(siguienteNivel))
        {
            Debug.Log("[DIAG-DESTINO] → Destino EXPLÍCITO: \"" + siguienteNivel + "\"");
            UnityEngine.SceneManagement.SceneManager.LoadScene(siguienteNivel);
            return;
        }

        // Si hay GameManager, usar su lógica de destino según nivel actual
        if (GameManager.Instance != null)
        {
            int nivelActual = GameManager.Instance.GetNivelActual();
            string destino = GameManager.Instance.GetSubLevelDestination();
            Debug.Log("[DIAG-DESTINO] → GameManager.nivelActual=" + nivelActual + " | GetSubLevelDestination()=\"" + destino + "\"");
            GameManager.Instance.AvanzarNivel();
            Debug.Log("[DIAG-DESTINO] → AvanzarNivel() ejecutado. Nuevo nivelActual=" + GameManager.Instance.GetNivelActual());
            Debug.Log("[DIAG-DESTINO] → Cargando escena: \"" + destino + "\"");
            UnityEngine.SceneManagement.SceneManager.LoadScene(destino);
            return;
        }

        // Fallback: siguiente build index
        int nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log("[DIAG-DESTINO] → Fallback: buildIndex " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + " → " + nextIndex);
        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);
        }
    }
}
