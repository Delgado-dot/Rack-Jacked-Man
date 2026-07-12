using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
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
    private string loadedPuzzleScene = "";
    private PuzzleUI puzzleUI;

    private void Start()
    {
        puzzleUI = FindAnyObjectByType<PuzzleUI>();
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

        PlayerMode playerMode = FindAnyObjectByType<PlayerMode>();
        if (playerMode != null)
        {
            playerMode.ChangeMode(PlayerMode.Mode.Puzzle);
        }

        string puzzleScene = ChoosePuzzleScene(rack.GetPuzzleName());
        Debug.Log("PuzzleManager: Cargando escena " + puzzleScene);

        puzzleTimer = puzzleTimeLimit;
        timerRunning = true;
        loadedPuzzleScene = puzzleScene;

        if (puzzleUI != null)
        {
            puzzleUI.ShowPuzzlePanel(rack.GetPuzzleName().ToUpper());
        }

        if (puzzleScene != null && SceneExists(puzzleScene))
        {
            SceneManager.LoadScene(puzzleScene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning("PuzzleManager: Escena '" + puzzleScene + "' no encontrada. Completando puzzle automaticamente.");
            PuzzleCompleted();
        }
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

        UnloadPuzzleScene();
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

        UnloadPuzzleScene();
        ReturnToMainScene();
    }

    public void ClosePuzzle()
    {
        if (!puzzleActive) return;
        PuzzleFailed();
    }

    private void UnloadPuzzleScene()
    {
        if (!string.IsNullOrEmpty(loadedPuzzleScene))
        {
            Scene scene = SceneManager.GetSceneByName(loadedPuzzleScene);
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(scene);
                Debug.Log("PuzzleManager: Escena " + loadedPuzzleScene + " descargada");
            }
            loadedPuzzleScene = "";
        }
    }

    private void ReturnToMainScene()
    {
        PlayerMode playerMode = FindAnyObjectByType<PlayerMode>();
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
