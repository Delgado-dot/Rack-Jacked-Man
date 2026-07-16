using UnityEngine;

public class SubLevelGameManager : MonoBehaviour
{
    public static SubLevelGameManager Instance { get; private set; }

    [Header("Datos de Escena")]
    [SerializeField] private CableSceneData sceneData;

    [Header("Referencias")]
    [SerializeField] private SubLevelPlayerController player;
    [SerializeField] private CableSubLevelManager cableManager;
    [SerializeField] private EnemySpawnerCable spawner;

    [Header("Estado")]
    [SerializeField] private bool isCompleted = false;
    [SerializeField] private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<SubLevelPlayerController>();
        if (cableManager == null)
            cableManager = FindAnyObjectByType<CableSubLevelManager>();
        if (spawner == null)
            spawner = FindAnyObjectByType<EnemySpawnerCable>();

        if (sceneData != null)
            sceneData.Initialize();

        if (player != null)
        {
            player.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    public void RegisterScore(int points)
    {
        if (isCompleted || isGameOver) return;
        if (player != null)
            player.AddScore(points);
    }

    private void HandlePlayerDeath()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (cableManager != null)
            cableManager.enabled = false;
        if (spawner != null)
            spawner.enabled = false;

        Debug.Log("[SubLevelGameManager] Game Over - spawner y cableManager deshabilitados");
    }

    public void CompleteLevel()
    {
        if (isCompleted) return;
        isCompleted = true;
        Debug.Log("[SubLevelGameManager] Nivel completado");
    }

    public CableSceneData GetSceneData() => sceneData;
    public bool IsCompleted() => isCompleted;
    public bool IsGameOver() => isGameOver;

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (player != null)
            player.OnPlayerDeath -= HandlePlayerDeath;
    }
}
