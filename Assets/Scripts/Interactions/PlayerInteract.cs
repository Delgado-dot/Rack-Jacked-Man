using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Configuracion de interaccion")]
    [SerializeField, Min(1.5f)] private float interactionRange = 2.5f;

    [Header("Referencia")]
    [SerializeField] private RackInteractable currentRack;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        // Algunas escenas antiguas guardaron 0.5 en el inspector, un alcance
        // menor que el tamano de los propios racks.
        interactionRange = Mathf.Max(interactionRange, 1.5f);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        DetectNearbyRack();

        if (currentRack != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void DetectNearbyRack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

        float closestDist = Mathf.Infinity;
        RackInteractable closest = null;

        foreach (Collider hit in hits)
        {
            // Los modelos importados suelen tener el collider en un hijo y el
            // componente interactuable en la raiz del rack.
            RackInteractable rack = hit.GetComponentInParent<RackInteractable>();
            if (rack == null)
                rack = hit.GetComponentInChildren<RackInteractable>();

            if (rack != null && rack.IsInteractable())
            {
                Vector3 targetPoint;
                if (hit is MeshCollider mc && !mc.convex)
                {
                    targetPoint = hit.bounds.ClosestPoint(transform.position);
                }
                else
                {
                    targetPoint = hit.ClosestPoint(transform.position);
                }

                float dist = Vector3.Distance(transform.position, targetPoint);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = rack;
                }
            }
        }

        if (closest != currentRack)
        {
            if (currentRack != null)
            {
                currentRack.OnHoverExit();
            }

            currentRack = closest;

            if (currentRack != null)
            {
                currentRack.OnHoverEnter();
            }
        }
    }

    private void Interact()
    {
        if (currentRack != null)
        {
            currentRack.Interact();
        }
    }

    public RackInteractable GetCurrentRack()
    {
        return currentRack;
    }

    public bool IsNearRack()
    {
        return currentRack != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
