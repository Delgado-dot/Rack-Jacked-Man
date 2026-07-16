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

        Debug.Log("PuertaSubLevel: Iniciado. doorIsOpen=" + doorIsOpen);
    }

    public void AbrirPuerta()
    {
        doorIsOpen = true;
        blockCollider.enabled = false;
        Debug.Log("PuertaSubLevel: Puerta ABIERTA");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!doorIsOpen)
        {
            Debug.Log("PuertaSubLevel: CERRADA, bloqueado.");
            return;
        }

        Debug.Log("PuertaSubLevel: Cargando " + nombreEscena);
        SceneManager.LoadScene(nombreEscena);
    }
}
