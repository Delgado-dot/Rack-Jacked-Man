using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton que controla el flujo del juego.
/// Maneja checkpoints, respawn, game over y completar nivel.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del juego")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isLevelCompleted = false;

    [Header("Referencias")]
    [SerializeField] private Transform lastCheckpoint;
    [SerializeField] private PlayerHealth playerHealth;

    private Vector3 initialSpawnPosition;

    private void Awake()
    {
        // Singleton - solo un GameManager en la escena
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Buscar jugador automaticamente
        if (playerHealth == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        // Guardar posicion inicial del jugador
        if (playerHealth != null)
        {
            initialSpawnPosition = playerHealth.transform.position;
        }

        // Buscar primer checkpoint (RackStart) si no hay ninguno registrado
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

    /// <summary>
    /// Registrar un checkpoint cuando el jugador lo toca.
    /// </summary>
    public void RegisterCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
        Debug.Log("Checkpoint registrado: " + checkpoint.name);
    }

    /// <summary>
    /// Respawn al ultimo checkpoint registrado.
    /// </summary>
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
            Debug.Log("Jugador respawneado en: " + respawnPosition);
        }
    }

    /// <summary>
    /// Ejecutar Game Over. Detiene el juego.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GAME OVER");

        // Aqui se puede agregar: mostrar UI de Game Over, reproducir sonido, etc.
    }

    /// <summary>
    /// Completar el nivel. Ejecutar al llegar a RackGoal.
    /// </summary>
    public void LevelCompleted()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        Debug.Log("NIVEL COMPLETADO!");

        // Aqui se puede agregar: mostrar pantalla de victoria, cargar siguiente nivel, etc.
    }

    /// <summary>
    /// Reiniciar la escena actual.
    /// </summary>
    public void RestartLevel()
    {
        isGameOver = false;
        isLevelCompleted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Getters
    public bool IsGameOver() { return isGameOver; }
    public bool IsLevelCompleted() { return isLevelCompleted; }
    public Transform GetLastCheckpoint() { return lastCheckpoint; }
}
