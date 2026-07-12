using UnityEngine;

/// <summary>
/// RackInteractable - Se adjunta a cada ServerRack para hacerlo interactuable.
/// Detecta proximidad del jugador y activa el puzzle.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RackInteractable : MonoBehaviour
{
    public enum RackType
    {
        Checkpoint,
        Final
    }

    [Header("Configuracion del rack")]
    [SerializeField] private RackType rackType = RackType.Checkpoint;
    [SerializeField] private int rackIndex = 0;
    [SerializeField] private string puzzleName = "cables";

    [Header("Estado")]
    [SerializeField] private bool repaired = false;
    [SerializeField] private bool puzzleActive = false;

    [Header("Referencias")]
    [SerializeField] private PuzzleManager puzzleManager;

    private Renderer rackRenderer;
    private Material originalMaterial;

    private void Start()
    {
        rackRenderer = GetComponent<Renderer>();
        if (rackRenderer != null)
        {
            originalMaterial = rackRenderer.sharedMaterial;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        if (puzzleManager == null)
        {
            puzzleManager = FindAnyObjectByType<PuzzleManager>();
        }
    }

    public bool IsInteractable()
    {
        return !repaired && !puzzleActive;
    }

    public void OnHoverEnter()
    {
        if (!IsInteractable()) return;
        Debug.Log("Hover sobre rack " + rackIndex + ": " + puzzleName);
    }

    public void OnHoverExit()
    {
        if (!IsInteractable()) return;
    }

    public void Interact()
    {
        if (!IsInteractable()) return;

        Debug.Log("Interactuando con rack " + rackIndex + ": " + puzzleName);
        puzzleActive = true;

        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle(this);
        }
    }

    public void Repair()
    {
        repaired = true;
        puzzleActive = false;
        Debug.Log("Rack " + rackIndex + " reparado!");

        if (GameManager.Instance != null)
        {
            if (rackType == RackType.Checkpoint)
            {
                GameManager.Instance.RegisterCheckpoint(transform);
            }
            else if (rackType == RackType.Final)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
    }

    public void OnPuzzleFailed()
    {
        puzzleActive = false;
        Debug.Log("Puzzle fallido en rack " + rackIndex);
    }

    public void SetPuzzleManager(PuzzleManager manager)
    {
        puzzleManager = manager;
    }

    public void SetPuzzleName(string name)
    {
        puzzleName = name;
    }

    public int GetRackIndex() { return rackIndex; }
    public RackType GetRackType() { return rackType; }
    public string GetPuzzleName() { return puzzleName; }
    public bool IsRepaired() { return repaired; }
}
