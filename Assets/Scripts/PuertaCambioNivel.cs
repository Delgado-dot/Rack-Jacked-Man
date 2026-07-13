using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaCambioNivel : MonoBehaviour
{
    public string nombreEscena = "Nivel_2";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }
}