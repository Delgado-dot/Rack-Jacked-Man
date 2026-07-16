using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Configuracion de interaccion")]
    [SerializeField] private float interactionRange = 1.2f;

    [Header("Referencia")]
    [SerializeField] private RackInteractable currentRack;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
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

        if (currentRack != null && Keyboard.current.eKey.wasPressedThisFrame)
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
            RackInteractable rack = hit.GetComponent<RackInteractable>();
            if (rack != null && rack.IsInteractable())
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
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
