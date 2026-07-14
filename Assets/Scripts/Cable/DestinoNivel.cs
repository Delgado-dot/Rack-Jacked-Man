using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinoNivel : MonoBehaviour
{
    [SerializeField] private string siguienteNivel = "";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[DestinoNivel] Nivel completado.");

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
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
    }
}
