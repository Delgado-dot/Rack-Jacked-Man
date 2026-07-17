using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaCambioNivel : MonoBehaviour
{
    public string nombreEscena = "Nivel_2";

    private bool doorIsOpen = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
        Debug.Log("[DIAG-PUERTACAMBIO] Start en \"" + gameObject.name + "\" | nombreEscena=\"" + nombreEscena + "\" | doorIsOpen=" + doorIsOpen);
    }

    public void SetDoorOpen(bool open)
    {
        doorIsOpen = open;
        Debug.Log("[DIAG-PUERTACAMBIO] SetDoorOpen(" + open + ") en \"" + gameObject.name + "\"");
    }

    private void OnTriggerEnter(Collider other)
    {
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("[DIAG-PUERTACAMBIO] OnTriggerEnter en \"" + gameObject.name + "\" | Escena: " + escenaActual + " | doorIsOpen=" + doorIsOpen + " | nombreEscena=\"" + nombreEscena + "\"");

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[DIAG-PUERTACAMBIO] → No es Player (" + other.gameObject.name + "). Ignorado.");
            return;
        }

        if (!doorIsOpen)
        {
            Debug.Log("[DIAG-PUERTACAMBIO] → PUERTA CERRADA. Bloqueado.");
            return;
        }

        Debug.Log("[DIAG-PUERTACAMBIO] → PUERTA ABIERTA. Cargando \"" + nombreEscena + "\"");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
    }
}