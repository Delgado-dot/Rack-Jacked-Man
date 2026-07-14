using UnityEngine;

/// <summary>
/// PlayerHealth - Sistema de vidas global del jugador.
/// Unico sistema de salud para todos los niveles.
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

    public event System.Action<int, int> OnHealthChanged;
    public event System.Action OnPlayerDeath;

    private Vector3 initialPosition;
    private float invulnerabilityTimer = 0f;
    private bool isInvulnerable = false;
    private bool isDead = false;
    private PlayerMovement playerMovement;

    private void Start()
    {
        AutoSetupRacks.Setup();

        initialPosition = transform.position;

        if (spawnPoint == null)
        {
            GameObject spawn = GameObject.Find("PlayerSpawn");
            if (spawn != null)
            {
                spawnPoint = spawn.transform;
            }
        }

        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
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
    /// Recibir 1 punto de danio.
    /// </summary>
    public void TakeDamage()
    {
        TakeDamage(1);
    }

    /// <summary>
    /// Recibir danio por cantidad. Reduce vidas y activa invulnerabilidad.
    /// Si las vidas llegan a 0, ejecuta Game Over.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isInvulnerable || isDead) return;

        currentLives -= amount;

        Debug.Log("[PlayerHealth] " + gameObject.name + " recibio -" + amount + " vida. Restante: " + currentLives);

        OnHealthChanged?.Invoke(currentLives, maxLives);

        if (currentLives <= 0)
        {
            currentLives = 0;
            Die();
            return;
        }

        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

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
            Debug.Log("[PlayerHealth] " + gameObject.name + " curado. Vidas actuales: " + currentLives);
            OnHealthChanged?.Invoke(currentLives, maxLives);
        }
    }

    /// <summary>
    /// Ejecutar muerte del jugador. Game Over.
    /// </summary>
    private void Die()
    {
        isDead = true;

        Debug.Log("[PlayerHealth] " + gameObject.name + " GAME OVER");

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        OnPlayerDeath?.Invoke();

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
            return;
        }

        Vector3 targetPosition = initialPosition;

        if (spawnPoint != null)
        {
            targetPosition = spawnPoint.position;
        }

        transform.position = targetPosition;
    }

    public void ResetDeath()
    {
        isDead = false;
        isInvulnerable = false;
        invulnerabilityTimer = 0f;
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    public int GetCurrentLives() { return currentLives; }
    public int GetMaxLives() { return maxLives; }
    public bool IsInvulnerable() { return isInvulnerable; }
    public bool IsDead() { return isDead; }
}
