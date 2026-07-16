using UnityEngine;

public class ObjectiveDoorController : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 4, 0);
    [SerializeField] private float blockWidth = 10f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    private RackInteractable[] allRacks;
    private Collider blockCollider;

    private void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;

        allRacks = FindObjectsByType<RackInteractable>();
        Debug.Log("ObjectiveDoor: Racks encontrados = " + allRacks.Length);

        CreateBlockZone();

        PuertaCambioNivel puerta = GetComponent<PuertaCambioNivel>();
        Debug.Log("ObjectiveDoor: PuertaCambioNivel = " + (puerta != null ? "SI" : "NO"));
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

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void CheckAndOpen()
    {
        if (isOpen) return;

        allRacks = FindObjectsByType<RackInteractable>();

        if (allRacks.Length == 0)
        {
            Debug.LogWarning("ObjectiveDoor: No se encontraron racks.");
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

        Debug.Log("ObjectiveDoor: " + repairedCount + "/" + allRacks.Length + " racks reparados");

        if (repairedCount >= allRacks.Length)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        isMoving = true;
        targetPosition = closedPosition + openOffset;
        blockCollider.enabled = false;

        PuertaSubLevel puerta = GetComponent<PuertaSubLevel>();
        if (puerta != null)
            puerta.AbrirPuerta();

        Debug.Log("ObjectiveDoor: Puerta abierta!");
    }

    public void CloseDoor()
    {
        isOpen = false;
        isMoving = true;
        targetPosition = closedPosition;
        blockCollider.enabled = true;
    }

    public bool IsOpen() { return isOpen; }
}
