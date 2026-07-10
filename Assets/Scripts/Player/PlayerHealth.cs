using UnityEngine;

/// <summary>
/// PlayerHealth - Sistema de vidas del jugador.
/// Controla danio, invulnerabilidad, respawn y Game Over.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Vidas")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives = 3;

    [Header("Invulnerabilidad")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Referencias")]
    [SerializeField] private Transform spawnPoint;

    private Vector3 initialPosition;
    private float invulnerabilityTimer = 0f;
    private bool isInvulnerable = false;
    private bool isDead = false;
    private PlayerMovement playerMovement;

    private void Start()
    {
        // Auto-setup racks y puzzles si no existen
        AutoSetupRacks.Setup();

        // Guardar posicion inicial para respawn
        initialPosition = transform.position;

        // Buscar spawn point por tag si no se asigno
        if (spawnPoint == null)
        {
            GameObject spawn = GameObject.Find("PlayerSpawn");
            if (spawn != null)
            {
                spawnPoint = spawn.transform;
            }
        }

        // Referencia al movimiento del jugador para detenerlo en Game Over
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Reducir timer de invulnerabilidad
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }

    /// <summary>
    /// Recibir danio. Reduce 1 vida y activa invulnerabilidad.
    /// Si las vidas llegan a 0, ejecuta Game Over.
    /// </summary>
    public void TakeDamage()
    {
        // No recibir danio si esta invulnerable o muerto
        if (isInvulnerable || isDead) return;

        currentLives--;

        Debug.Log("Jugador recibio danio. Vidas restantes: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        // Activar invulnerabilidad
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        // Regresar al punto de aparicion
        Respawn();
    }

    /// <summary>
    /// Curar al jugador. Aumenta 1 vida hasta el maximo.
    /// </summary>
    public void Heal()
    {
        if (isDead) return;

        if (currentLives < maxLives)
        {
            currentLives++;
            Debug.Log("Jugador curado. Vidas actuales: " + currentLives);
        }
    }

    /// <summary>
    /// Ejecutar muerte del jugador. Game Over.
    /// </summary>
    private void Die()
    {
        isDead = true;
        currentLives = 0;

        Debug.Log("GAME OVER");

        // Detener movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// Regresar al punto de aparicion o ultimo checkpoint.
    /// </summary>
    private void Respawn()
    {
        // Usar GameManager si esta disponible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
            return;
        }

        // Fallback: usar posicion inicial o spawn point
        Vector3 targetPosition = initialPosition;

        if (spawnPoint != null)
        {
            targetPosition = spawnPoint.position;
        }

        transform.position = targetPosition;
    }

    // Getters para UI o otros sistemas
    public int GetCurrentLives() { return currentLives; }
    public int GetMaxLives() { return maxLives; }
    public bool IsInvulnerable() { return isInvulnerable; }
    public bool IsDead() { return isDead; }
}
