using UnityEngine;

public class ObjectiveDoorController : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float blockWidth = 10f;
    [SerializeField] private Color colorAbierto = new Color(0f, 0.5f, 1f, 1f);

    private bool isOpen = false;
    private RackInteractable[] allRacks;
    private Collider blockCollider;
    private MeshRenderer doorRenderer;
    private Color colorOriginal;

    private void Start()
    {
        doorRenderer = GetComponent<MeshRenderer>();
        if (doorRenderer != null)
            colorOriginal = doorRenderer.material.color;

        allRacks = FindObjectsByType<RackInteractable>();
        Debug.Log("[DIAG-DOOR] Start en \"" + gameObject.name + "\" | Racks=" + allRacks.Length);

        CreateBlockZone();

        PuertaSubLevel subPuerta = GetComponent<PuertaSubLevel>();
        Debug.Log("[DIAG-DOOR] PuertaSubLevel=" + (subPuerta != null ? "SI" : "NO"));
    }

    private void CreateBlockZone()
    {
        GameObject zone = new GameObject("DoorBlockZone");
        zone.transform.SetParent(transform);
        zone.transform.localPosition = Vector3.zero;
        zone.transform.localRotation = Quaternion.identity;

        BoxCollider col = zone.AddComponent<BoxCollider>();
        col.size = new Vector3(blockWidth, 5f, 2f);
        col.center = Vector3.zero;
        col.isTrigger = false;
        blockCollider = col;
    }

    public void CheckAndOpen()
    {
        if (isOpen)
        {
            Debug.Log("[DIAG-DOOR] CheckAndOpen: Ya abierto. Ignorado.");
            return;
        }

        allRacks = FindObjectsByType<RackInteractable>();

        if (allRacks.Length == 0)
        {
            Debug.LogWarning("[DIAG-DOOR] CheckAndOpen: No se encontraron racks.");
            return;
        }

        int repairedCount = 0;
        foreach (RackInteractable rack in allRacks)
        {
            if (rack != null && rack.IsRepaired())
            {
                repairedCount++;
            }
        }

        Debug.Log("[DIAG-DOOR] CheckAndOpen: " + repairedCount + "/" + allRacks.Length + " racks reparados");

        if (repairedCount >= allRacks.Length)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;

        if (doorRenderer != null)
            doorRenderer.material.color = colorAbierto;

        blockCollider.enabled = false;

        Collider[] allCols = GetComponents<Collider>();
        foreach (Collider c in allCols)
        {
            if (!c.isTrigger)
                c.enabled = false;
        }

        PuertaSubLevel puerta = GetComponent<PuertaSubLevel>();
        if (puerta != null)
        {
            Debug.Log("[DIAG-DOOR] OpenDoor: Llamando PuertaSubLevel.AbrirPuerta()");
            puerta.AbrirPuerta();
        }

        PuertaCambioNivel puertaCambio = GetComponent<PuertaCambioNivel>();
        if (puertaCambio != null)
        {
            puertaCambio.SetDoorOpen(true);
            Debug.Log("[DIAG-DOOR] OpenDoor: PuertaCambioNivel abierta");
        }

        if (puerta == null && puertaCambio == null)
        {
            Debug.LogWarning("[DIAG-DOOR] OpenDoor: No hay PuertaSubLevel ni PuertaCambioNivel");
        }

        Debug.Log("[DIAG-DOOR] OpenDoor: Puerta abierta!");
    }

    public void CloseDoor()
    {
        isOpen = false;

        if (doorRenderer != null)
            doorRenderer.material.color = colorOriginal;

        blockCollider.enabled = true;

        Collider originalCol = GetComponent<Collider>();
        if (originalCol != null)
            originalCol.enabled = true;
    }

    public bool IsOpen() { return isOpen; }
}
