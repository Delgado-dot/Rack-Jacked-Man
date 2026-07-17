using UnityEngine;
using UnityEngine.SceneManagement;

public class RackExitTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Menu Victoria";
    [SerializeField] private float delayAfterAllRepaired = 1.5f;

    private BoxCollider triggerCol;
    private bool activated = false;
    private float timer = -1f;

    private void Start()
    {
        triggerCol = GetComponent<BoxCollider>();
        if (triggerCol == null)
        {
            triggerCol = gameObject.AddComponent<BoxCollider>();
        }
        triggerCol.isTrigger = true;
        triggerCol.enabled = false;
    }

    private void Update()
    {
        if (activated) return;

        if (AllRacksRepaired())
        {
            timer += Time.deltaTime;
            if (timer >= delayAfterAllRepaired)
            {
                activated = true;
                triggerCol.enabled = true;
                Debug.Log("[RackExitTrigger] Todos los racks reparados. Salida activada.");
            }
        }
        else
        {
            timer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activated) return;
        if (!other.CompareTag("Player")) return;

        Debug.Log("[RackExitTrigger] Cargando " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }

    private bool AllRacksRepaired()
    {
        RackInteractable[] racks = FindObjectsByType<RackInteractable>();
        if (racks.Length == 0) return false;

        foreach (var rack in racks)
        {
            if (!rack.IsRepaired()) return false;
        }
        return true;
    }
}
