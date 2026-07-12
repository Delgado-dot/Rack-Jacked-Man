using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PuzzleManager - Orquestador de minijuegos Unity.
/// Reemplaza al PythonPuzzleManager para puzzles dentro de Unity.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMode playerMode;
    [SerializeField] private PuzzleUI puzzleUI;

    [Header("Puzzles disponibles")]
    [SerializeField] private string[] puzzleScenes = {
        "Puzzle_Cables",
        "Puzzle_Dispatcher",
        "Puzzle_Nave",
        "Puzzle_Trafico",
        "Puzzle_PatchCore"
    };

    [Header("Tiempo")]
    [SerializeField] private float puzzleTimeLimit = 30f;

    private bool puzzleActive = false;
    private RackInteractable currentRack;
    private float puzzleTimer = 0f;
    private bool timerRunning = false;

    private void Start()
    {
        if (playerMode == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMode = player.GetComponent<PlayerMode>();
            }
        }

        if (puzzleUI == null)
        {
            puzzleUI = FindAnyObjectByType<PuzzleUI>();
        }
    }

    private void Update()
    {
        if (!puzzleActive || !timerRunning) return;

        puzzleTimer -= Time.deltaTime;

        if (puzzleUI != null)
        {
            puzzleUI.UpdateTimer(puzzleTimer);
        }

        if (puzzleTimer <= 0f)
        {
            PuzzleFailed();
        }
    }

    public void StartPuzzle(RackInteractable rack)
    {
        if (puzzleActive) return;

        currentRack = rack;
        puzzleActive = true;

        Debug.Log("PuzzleManager: Iniciando puzzle para rack " + rack.GetRackIndex());

        if (playerMode != null)
        {
            playerMode.ChangeMode(PlayerMode.Mode.Puzzle);
        }

        string puzzleScene = ChoosePuzzleScene(rack.GetPuzzleName());
        Debug.Log("PuzzleManager: Cargando escena " + puzzleScene);

        puzzleTimer = puzzleTimeLimit;
        timerRunning = true;

        if (puzzleUI != null)
        {
            puzzleUI.ShowPuzzlePanel("Rack " + rack.GetRackIndex() + " - " + rack.GetPuzzleName());
        }

        if (puzzleScene != null && SceneExists(puzzleScene))
        {
            SceneManager.sceneLoaded += OnPuzzleSceneLoaded;
            SceneManager.LoadScene(puzzleScene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning("PuzzleManager: Escena '" + puzzleScene + "' no encontrada. Completando puzzle automaticamente.");
            PuzzleCompleted();
        }
    }

    private void OnPuzzleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnPuzzleSceneLoaded;
        Debug.Log("PuzzleManager: Escena de puzzle cargada: " + scene.name);
    }

    public void PuzzleCompleted()
    {
        if (!puzzleActive) return;

        puzzleActive = false;
        timerRunning = false;

        Debug.Log("PuzzleManager: Puzzle completado!");

        if (puzzleUI != null)
        {
            puzzleUI.HidePuzzlePanel();
        }

        if (currentRack != null)
        {
            currentRack.Repair();
        }

        ReturnToMainScene();
    }

    public void PuzzleFailed()
    {
        if (!puzzleActive) return;

        puzzleActive = false;
        timerRunning = false;

        Debug.Log("PuzzleManager: Puzzle fallido!");

        if (puzzleUI != null)
        {
            puzzleUI.HidePuzzlePanel();
        }

        if (currentRack != null)
        {
            currentRack.OnPuzzleFailed();
        }

        ReturnToMainScene();
    }

    private void ReturnToMainScene()
    {
        if (playerMode != null)
        {
            playerMode.ChangeMode(PlayerMode.Mode.Normal);
        }

        currentRack = null;
    }

    private string ChoosePuzzleScene(string puzzleName)
    {
        if (puzzleScenes == null || puzzleScenes.Length == 0) return null;

        foreach (string scene in puzzleScenes)
        {
            if (scene.ToLower().Contains(puzzleName.ToLower()))
            {
                return scene;
            }
        }

        return puzzleScenes[Random.Range(0, puzzleScenes.Length)];
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPuzzleActive() { return puzzleActive; }
}
