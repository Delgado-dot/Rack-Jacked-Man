using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton que controla el flujo del juego.
/// Maneja checkpoints, respawn, game over y completar nivel.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Escenas")]
    public string sceneSubLevel = "SubCable01_Copy";
    public string sceneGameOver = "Menu GameOver";
    public string sceneVictory = "Menu Victoria";
    public string sceneMainMenu = "MenuPrincipal";

    [Header("Estado del juego")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isLevelCompleted = false;

    [Header("Referencias")]
    [SerializeField] private Transform lastCheckpoint;
    [SerializeField] private PlayerHealth playerHealth;

    private Vector3 initialSpawnPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindPlayerHealth();

        if (lastCheckpoint == null)
        {
            GameObject rackStart = GameObject.Find("RackStart");
            if (rackStart != null)
            {
                lastCheckpoint = rackStart.transform;
                initialSpawnPosition = rackStart.transform.position;
            }
        }
    }

    private void FindPlayerHealth()
    {
        if (playerHealth != null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            initialSpawnPosition = player.transform.position;
        }
    }

    public void RegisterCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
        Debug.Log("Checkpoint registrado: " + checkpoint.name);
    }

    public void RespawnPlayer()
    {
        if (isGameOver) return;

        if (playerHealth == null)
            FindPlayerHealth();

        Vector3 respawnPosition = initialSpawnPosition;

        if (lastCheckpoint != null)
        {
            respawnPosition = lastCheckpoint.position;
        }

        if (playerHealth != null)
        {
            playerHealth.transform.position = respawnPosition;
            PlayerHealth.ResetDeath();
            Debug.Log("Jugador respawneado en: " + respawnPosition);
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GAME OVER");
        SafeLoadScene(sceneGameOver);
    }

    public void LevelCompleted()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        Debug.Log("NIVEL COMPLETADO!");
        SafeLoadScene(sceneVictory);
    }

    public void LoadSubLevel()
    {
        Debug.Log("Cargando subnivel: " + sceneSubLevel);
        isLevelCompleted = false;
        SafeLoadScene(sceneSubLevel);
    }

    public void LoadMainMenu()
    {
        isGameOver = false;
        isLevelCompleted = false;
        Time.timeScale = 1f;
        SafeLoadScene(sceneMainMenu);
    }

    public void RestartLevel()
    {
        isGameOver = false;
        isLevelCompleted = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetState()
    {
        isGameOver = false;
        isLevelCompleted = false;
        Time.timeScale = 1f;
    }

    private void SafeLoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameManager] Nombre de escena vacio.");
            return;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        Debug.LogError("[GameManager] La escena '" + sceneName + "' NO esta en Build Settings." +
            "\nVe a File > Build Profiles y agregala." +
            "\nO ejecuta: Rack-Jacked-Man > Setup Build Scenes");
    }

    public bool IsGameOver() { return isGameOver; }
    public bool IsLevelCompleted() { return isLevelCompleted; }
    public Transform GetLastCheckpoint() { return lastCheckpoint; }
}
