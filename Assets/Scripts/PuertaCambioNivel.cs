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
        Debug.Log("PuertaCambioNivel: Trigger listo. doorIsOpen = " + doorIsOpen);
    }

    public void SetDoorOpen(bool open)
    {
        doorIsOpen = open;
        Debug.Log("PuertaCambioNivel: doorIsOpen = " + doorIsOpen);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PuertaCambioNivel: OnTriggerEnter con " + other.gameObject.name + " | doorIsOpen=" + doorIsOpen);

        if (!other.CompareTag("Player")) return;

        if (!doorIsOpen)
        {
            Debug.Log("PuertaCambioNivel: Puerta CERRADA, bloqueado.");
            return;
        }

        Debug.Log("PuertaCambioNivel: Puerta ABIERTA, cargando " + nombreEscena);
        SceneManager.LoadScene(nombreEscena);
    }
}