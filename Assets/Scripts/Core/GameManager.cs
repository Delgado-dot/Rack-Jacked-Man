using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton que controla el flujo del juego.
/// Maneja checkpoints, respawn, game over y completar nivel.
/// Persiste: vidas, puntos, nivel actual, chaquetas usadas.
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

    // Persistencia entre niveles
    private static int s_nivelActual = 1;
    private static int s_puntos = 0;
    private static int s_chaquetasUsadas = 0;

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
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == sceneMainMenu)
        {
            ResetPersistencia();
        }

        if (playerHealth == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (playerHealth != null)
        {
            initialSpawnPosition = playerHealth.transform.position;
        }

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

    public void RegisterCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
        Debug.Log("Checkpoint registrado: " + checkpoint.name);
    }

    public void RespawnPlayer()
    {
        if (isGameOver) return;

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
        Time.timeScale = 0f;
        SafeLoadScene(sceneGameOver);
    }

    public void LevelCompleted()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        Debug.Log("NIVEL COMPLETADO!");
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
        ResetPersistencia();
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
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        Debug.LogError("[GameManager] La escena '" + sceneName + "' NO esta en Build Settings." +
            "\nVe a File > Build Profiles y agregala." +
            "\nO ejecuta: Rack-Jacked-Man > Setup Build Scenes");
    }

    // ─── Persistencia ─────────────────────────────────────────────

    public int GetNivelActual() => s_nivelActual;
    public void SetNivelActual(int nivel) => s_nivelActual = nivel;

    public int GetPuntos() => s_puntos;
    public void AddPuntos(int cantidad) => s_puntos += cantidad;

    public int GetChaquetasUsadas() => s_chaquetasUsadas;
    public void AddChaquetaUsada() => s_chaquetasUsadas++;

    public void ResetPersistencia()
    {
        s_nivelActual = 1;
        s_puntos = 0;
        s_chaquetasUsadas = 0;
        PlayerHealth.ResetForNewScene();
    }

    /// <summary>
    /// Retorna la escena destino del SubCable según el nivel actual.
    /// Nivel 1 -> SubCable -> Nivel_2
    /// Nivel 2 -> SubCable -> Nivel_3
    /// Nivel 3 -> SubCable -> Victoria
    /// </summary>
    public string GetSubLevelDestination()
    {
        switch (s_nivelActual)
        {
            case 1: return "Nivel_2";
            case 2: return "Nivel_3";
            default: return sceneVictory;
        }
    }

    public void AvanzarNivel()
    {
        s_nivelActual++;
        Debug.Log("[GameManager] Nivel actual: " + s_nivelActual);
    }

    public bool IsGameOver() { return isGameOver; }
    public bool IsLevelCompleted() { return isLevelCompleted; }
    public Transform GetLastCheckpoint() { return lastCheckpoint; }
}
