using UnityEngine;

public class RackInteractable : MonoBehaviour
{
    public enum RackType { Checkpoint, Final }

    [Header("Configuracion")]
    [SerializeField] private RackType rackType = RackType.Checkpoint;
    [SerializeField] private int rackIndex = 0;

    [Header("Estado")]
    [SerializeField] private bool repaired = false;
    [SerializeField] private bool puzzleActive = false;

    [Header("Referencias")]
    [SerializeField] private PuzzleMana puzzleMana;
    [SerializeField] private RackState rackState;
    [SerializeField] private ObjectiveDoorController doorController;

    private Renderer rackRenderer;

    private void Start()
    {
        rackRenderer = GetComponent<Renderer>();

        if (puzzleMana == null)
            puzzleMana = FindAnyObjectByType<PuzzleMana>();

        if (rackState == null)
            rackState = GetComponent<RackState>();

        if (doorController == null)
            doorController = FindAnyObjectByType<ObjectiveDoorController>();
    }

    public bool IsInteractable()
    {
        return !repaired && !puzzleActive;
    }

    public void OnHoverEnter()
    {
        if (!IsInteractable()) return;
        Debug.Log("Hover sobre rack " + rackIndex);
    }

    public void OnHoverExit()
    {
        if (!IsInteractable()) return;
    }

    public void Interact()
    {
        if (!IsInteractable()) return;
        if (puzzleMana == null)
        {
            Debug.LogWarning("RackInteractable: PuzzleMana no encontrado");
            return;
        }

        Debug.Log("RackInteractable: Iniciando puzzle para rack " + rackIndex);
        puzzleActive = true;
        puzzleMana.StartPuzzle(this);
    }

    public void Repair()
    {
        if (repaired) return;

        repaired = true;
        puzzleActive = false;
        Debug.Log("Rack " + rackIndex + " reparado!");

        if (rackState != null)
            rackState.SetRepaired(true);

        if (rackType == RackType.Checkpoint && GameManager.Instance != null)
            GameManager.Instance.RegisterCheckpoint(transform);
        else if (rackType == RackType.Final && GameManager.Instance != null)
            GameManager.Instance.LevelCompleted();

        if (doorController == null)
            doorController = FindAnyObjectByType<ObjectiveDoorController>();

        if (doorController != null)
            doorController.CheckAndOpen();
    }

    public void OnPuzzleFailed()
    {
        puzzleActive = false;
        Debug.Log("Puzzle fallido en rack " + rackIndex);
    }

    public int GetRackIndex() { return rackIndex; }
    public RackType GetRackType() { return rackType; }
    public bool IsRepaired() { return repaired; }
    public string GetPuzzleName() { return "random"; }
}
