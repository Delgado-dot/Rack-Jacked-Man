using UnityEngine;
using UnityEngine.SceneManagement;

public static class SpawnJugadorPorNivel
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ColocarJugadorEnElNivel()
    {
        string nombreNivel = SceneManager.GetActiveScene().name;
        if (nombreNivel != "Nivel_2" && nombreNivel != "Nivel_3")
            return;

        GameObject jugador = BuscarJugadorReal();
        if (jugador == null)
        {
            Debug.LogError("No se encontró el objeto Player al cargar " + nombreNivel + ".");
            return;
        }

        Vector3 posicionInterior = nombreNivel == "Nivel_2"
            ? new Vector3(-3.75f, 3.5f, 8.1f)
            : new Vector3(4.285f, 2.017f, 30.808f);
        jugador.transform.SetPositionAndRotation(posicionInterior, Quaternion.identity);

        Rigidbody rigidbodyJugador = jugador.GetComponent<Rigidbody>();
        if (rigidbodyJugador != null)
        {
            rigidbodyJugador.position = posicionInterior;
            rigidbodyJugador.linearVelocity = Vector3.zero;
            rigidbodyJugador.angularVelocity = Vector3.zero;
        }
    }

    private static GameObject BuscarJugadorReal()
    {
        GameObject[] objetosPlayer = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject objeto in objetosPlayer)
        {
            if (objeto.name == "Player" && objeto.GetComponent<Rigidbody>() != null)
                return objeto;
        }

        return null;
    }
}
