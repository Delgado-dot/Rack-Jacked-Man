using UnityEngine;

public class PuzzleSceneBase : MonoBehaviour
{
    [Header("Configuracion del puzzle")]
    [SerializeField] protected string puzzleName = "Puzzle";
    [SerializeField] protected float timeLimit = 30f;

    protected float timer;
    protected bool puzzleCompleted = false;
    protected bool puzzleFailed = false;

    protected virtual void Start()
    {
        timer = timeLimit;
        Time.timeScale = 1f;
        Debug.Log("PuzzleSceneBase: " + puzzleName + " iniciado");
    }

    protected virtual void Update()
    {
        if (puzzleCompleted || puzzleFailed) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Fail();
        }
    }

    public virtual void Complete()
    {
        if (puzzleCompleted || puzzleFailed) return;

        puzzleCompleted = true;
        Debug.Log("Puzzle completado: " + puzzleName);

        PuzzleManager puzzleManager = FindAnyObjectByType<PuzzleManager>();
        if (puzzleManager != null)
        {
            puzzleManager.PuzzleCompleted();
        }
    }

    public virtual void Fail()
    {
        if (puzzleCompleted || puzzleFailed) return;

        puzzleFailed = true;
        Debug.Log("Puzzle fallido: " + puzzleName);

        PuzzleManager puzzleManager = FindAnyObjectByType<PuzzleManager>();
        if (puzzleManager != null)
        {
            puzzleManager.PuzzleFailed();
        }
    }

    public float GetTimeRemaining() { return timer; }
    public bool IsCompleted() { return puzzleCompleted; }
    public bool IsFailed() { return puzzleFailed; }
}
