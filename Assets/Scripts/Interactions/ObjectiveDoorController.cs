using UnityEngine;

public class ObjectiveDoorController : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 4, 0);

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    private RackInteractable[] allRacks;

    private void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;

        allRacks = FindObjectsByType<RackInteractable>();
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

        if (allRacks == null || allRacks.Length == 0)
        {
            allRacks = FindObjectsByType<RackInteractable>();
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
        Debug.Log("ObjectiveDoor: Puerta abierta!");
    }

    public void CloseDoor()
    {
        isOpen = false;
        isMoving = true;
        targetPosition = closedPosition;
    }

    public bool IsOpen() { return isOpen; }
}
