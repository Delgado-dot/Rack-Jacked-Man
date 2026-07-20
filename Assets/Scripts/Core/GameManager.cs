using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    [Header("Tiempo del nivel")]
    [SerializeField] private bool timerEnabled = true;
    [SerializeField, Min(1f)] private float defaultTimeLimit = 300f;

    [Header("Referencias")]
    [SerializeField] private Transform lastCheckpoint;
    [SerializeField] private PlayerHealth playerHealth;

    private Vector3 initialSpawnPosition;

    // Persistencia entre niveles
    private static int s_nivelActual = 1;
    private static int s_puntos = 0;
    private static int s_chaquetasUsadas = 0;
    private static bool s_nivelAvanzado = false;
    private static string s_lastPlayedLevel = "";
    private static float s_timeLimit = 300f;
    private static float s_timeRemaining = 300f;

    private bool timerRunning = false;
    private bool timerCompleted = false;
    private int lastDisplayedSecond = -1;

    public static event Action<float, float> OnTimeChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
        s_nivelActual = 1;
        s_puntos = 0;
        s_chaquetasUsadas = 0;
        s_nivelAvanzado = false;
        s_lastPlayedLevel = "";
        s_timeLimit = 300f;
        s_timeRemaining = 300f;
        OnTimeChanged = null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            s_timeLimit = Mathf.Max(1f, defaultTimeLimit);
            if (s_timeRemaining <= 0f || s_timeRemaining > s_timeLimit)
                s_timeRemaining = s_timeLimit;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefrescarReferenciasDeEscena(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (!timerEnabled || !timerRunning || isGameOver || isLevelCompleted)
            return;

        s_timeRemaining = Mathf.Max(0f, s_timeRemaining - Time.deltaTime);
        NotifyTimeChanged();

        if (s_timeRemaining <= 0f)
        {
            timerRunning = false;
            Debug.Log("[GameManager] Tiempo agotado.");
            GameOver();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Los puzzles se cargan de forma aditiva sobre el nivel actual. No son
        // un cambio de nivel y no deben reiniciar vidas, referencias ni tiempo.
        if (mode == LoadSceneMode.Additive) return;

        RefrescarReferenciasDeEscena(scene.name);
    }

    private void RefrescarReferenciasDeEscena(string escenaActual)
    {
        s_nivelAvanzado = false;
        playerHealth = null;
        lastCheckpoint = null;

        if (escenaActual == sceneMainMenu)
        {
            ResetPersistencia();
        }

        // Track last gameplay scene for restart functionality
        if (escenaActual != sceneMainMenu &&
            escenaActual != sceneGameOver &&
            escenaActual != sceneVictory)
        {
            s_lastPlayedLevel = escenaActual;
        }

        if (playerHealth == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }

            // Algunas escenas usan otro nombre para el objeto del jugador.
            // Buscar el componente evita detener el reloj por ese detalle.
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
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

        bool esEscenaJugable = playerHealth != null &&
            escenaActual != sceneMainMenu &&
            escenaActual != sceneGameOver &&
            escenaActual != sceneVictory;

        if (esEscenaJugable)
        {
            // Cada escena jugable comienza con estado y tiempo propios, incluso
            // si la anterior termino mediante una puerta que llamo LoadScene.
            isGameOver = false;
            isLevelCompleted = false;
            StartLevelTimer();
        }
        else
            StopLevelTimer();
    }

    public void StartLevelTimer()
    {
        s_timeLimit = Mathf.Max(1f, defaultTimeLimit);
        s_timeRemaining = s_timeLimit;
        timerRunning = timerEnabled;
        timerCompleted = false;
        lastDisplayedSecond = -1;
        NotifyTimeChanged(true);
    }

    public void EnsureLevelTimerStarted()
    {
        if (!timerRunning && !timerCompleted && !isGameOver && !isLevelCompleted)
            StartLevelTimer();
    }

    public void StopLevelTimer()
    {
        timerRunning = false;
        NotifyTimeChanged(true);
    }

    public void ResetLevelTimer()
    {
        s_timeLimit = Mathf.Max(1f, defaultTimeLimit);
        s_timeRemaining = s_timeLimit;
        timerRunning = timerEnabled;
        timerCompleted = false;
        lastDisplayedSecond = -1;
        NotifyTimeChanged(true);
    }

    public void AddTime(float seconds)
    {
        s_timeRemaining = Mathf.Clamp(s_timeRemaining + seconds, 0f, s_timeLimit);
        NotifyTimeChanged(true);
    }

    private void NotifyTimeChanged(bool force = false)
    {
        int displayedSecond = Mathf.CeilToInt(s_timeRemaining);
        if (!force && displayedSecond == lastDisplayedSecond) return;

        lastDisplayedSecond = displayedSecond;
        OnTimeChanged?.Invoke(s_timeRemaining, s_timeLimit);
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
        timerRunning = false;
        Debug.Log("GAME OVER");
        Time.timeScale = 0f;
        SafeLoadScene(sceneGameOver);
    }

    public void LevelCompleted()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        timerRunning = false;
        timerCompleted = true;
        NotifyTimeChanged(true);
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
        ResetLevelTimer();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetState()
    {
        isGameOver = false;
        isLevelCompleted = false;
        Time.timeScale = 1f;
        PlayerHealth.ResetForNewScene();
        ResetLevelTimer();
    }

    public string GetLastPlayedLevel()
    {
        return s_lastPlayedLevel;
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
        s_nivelAvanzado = false;
        s_lastPlayedLevel = "";
        PlayerHealth.ResetForNewScene();
        ResetLevelTimer();
    }

    /// <summary>
    /// Retorna la escena destino del SubCable según el nivel actual.
    /// Nivel 1 -> SubCable -> Nivel_2
    /// Nivel 2 -> SubCable -> Nivel_3
    /// Nivel 3 -> SubCable -> Menu Victoria (respaldo, no parte del flujo normal)
    /// </summary>
    public string GetSubLevelDestination()
    {
        switch (s_nivelActual)
        {
            case 1: return "Nivel_2";
            case 2: return "Nivel_3";
            case 3: return sceneVictory;
            default: return sceneVictory;
        }
    }

    public void AvanzarNivel()
    {
        if (s_nivelAvanzado)
        {
            Debug.Log("[GameManager] Nivel ya avanzado recientemente. Ignorado.");
            return;
        }
        s_nivelAvanzado = true;
        s_nivelActual++;
        Debug.Log("[GameManager] Nivel actual: " + s_nivelActual);
    }

    public bool IsGameOver() { return isGameOver; }
    public bool IsLevelCompleted() { return isLevelCompleted; }
    public Transform GetLastCheckpoint() { return lastCheckpoint; }
    public float GetTimeRemaining() { return s_timeRemaining; }
    public float GetTimeLimit() { return s_timeLimit; }
    public bool IsTimerRunning() { return timerRunning; }
}
