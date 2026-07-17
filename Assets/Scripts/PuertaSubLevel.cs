using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaSubLevel : MonoBehaviour
{
    public string nombreEscena = "SubCable01_Copy";
    public float blockWidth = 10f;

    private bool doorIsOpen = false;
    private Collider blockCollider;

    private void Start()
    {
        GameObject block = new GameObject("BlockZone");
        block.transform.SetParent(transform);
        block.transform.localPosition = Vector3.zero;
        blockCollider = block.AddComponent<BoxCollider>();
        ((BoxCollider)blockCollider).size = new Vector3(blockWidth, 5f, 2f);
        blockCollider.isTrigger = false;

        Collider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        Debug.Log("[DIAG-PUERTASUB] Start en \"" + gameObject.name + "\" | nombreEscena=\"" + nombreEscena + "\" | doorIsOpen=" + doorIsOpen);
    }

    public void AbrirPuerta()
    {
        doorIsOpen = true;
        blockCollider.enabled = false;
        Debug.Log("[DIAG-PUERTASUB] AbrirPuerta() llamado en \"" + gameObject.name + "\" | doorIsOpen=" + doorIsOpen);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("[DIAG-PUERTASUB] OnTriggerEnter en \"" + gameObject.name + "\" | Escena: " + escenaActual + " | doorIsOpen=" + doorIsOpen + " | nombreEscena=\"" + nombreEscena + "\"");

        if (!doorIsOpen)
        {
            Debug.Log("[DIAG-PUERTASUB] → PUERTA CERRADA. Bloqueado.");
            return;
        }

        Debug.Log("[DIAG-PUERTASUB] → PUERTA ABIERTA. Cargando \"" + nombreEscena + "\"");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
    }
}
