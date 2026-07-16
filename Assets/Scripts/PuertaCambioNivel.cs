using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaCambioNivel : MonoBehaviour
{
    [SerializeField] private string escenaDestino = "Nivel_3";
    private bool cambiandoNivel;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || cambiandoNivel)
            return;

        Debug.Log(
            "Entró al trigger: " + other.name +
            " | Tag: " + other.tag
        );

        bool esJugador =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player");

        if (!esJugador)
        {
            Debug.LogWarning("El objeto que entró no tiene el Tag Player.");
            return;
        }

        if (string.IsNullOrWhiteSpace(escenaDestino))
        {
            Debug.LogError("La puerta no tiene una escena de destino configurada.", this);
            return;
        }

        escenaDestino = escenaDestino.Trim();

        if (!Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError(
                "No se puede cargar la escena: " + escenaDestino +
                ". Revisa Build Profiles y el nombre."
            );
            return;
        }

        cambiandoNivel = true;
        Debug.Log("Cargando escena: " + escenaDestino);
        SceneManager.LoadScene(escenaDestino);
    }
}
