using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vidas (inspector)")]
    [SerializeField] private int maxLives = 3;

    [Header("Invulnerabilidad")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Referencias")]
    [SerializeField] private Transform spawnPoint;

    private static int s_maxLives = 3;
    private static int s_currentLives = 3;
    private static bool s_isInvulnerable = false;
    private static bool s_isDead = false;
    private static float s_invulnerabilityTimer = 0f;
    private static float s_invulnerabilityDuration = 1f;

    public static event System.Action<int, int> OnHealthChanged;
    public static event System.Action OnPlayerDeath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        s_maxLives = 3;
        s_currentLives = 3;
        s_isInvulnerable = false;
        s_isDead = false;
        s_invulnerabilityTimer = 0f;
        s_invulnerabilityDuration = 1f;
        OnHealthChanged = null;
        OnPlayerDeath = null;
    }

    private PlayerMovement playerMovement;
    private SubLevelPlayerController subLevelController;

    private void Awake()
    {
        s_maxLives = Mathf.Max(1, maxLives);
        s_invulnerabilityDuration = invulnerabilityDuration;

        if (s_currentLives <= 0 || s_currentLives > s_maxLives)
            s_currentLives = s_maxLives;

        s_isDead = false;
        s_isInvulnerable = false;
        s_invulnerabilityTimer = 0f;

        playerMovement = GetComponent<PlayerMovement>();
        subLevelController = GetComponent<SubLevelPlayerController>();
    }

    private void Start()
    {
        AutoSetupRacks.Setup();

        if (spawnPoint == null)
        {
            GameObject spawn = GameObject.Find("PlayerSpawn");
            if (spawn != null)
                spawnPoint = spawn.transform;
        }

        OnHealthChanged?.Invoke(s_currentLives, s_maxLives);
    }

    private void Update()
    {
        if (s_isInvulnerable)
        {
            s_invulnerabilityTimer -= Time.deltaTime;
            if (s_invulnerabilityTimer <= 0f)
                s_isInvulnerable = false;
        }
    }

    public static void TakeDamage(int amount)
    {
        if (s_isInvulnerable || s_isDead) return;

        amount = Mathf.Max(0, amount);
        if (amount == 0) return;

        s_currentLives = Mathf.Max(0, s_currentLives - amount);

        Debug.Log("[PlayerHealth] danio -" + amount + ". Restante: " + s_currentLives);

        OnHealthChanged?.Invoke(s_currentLives, s_maxLives);

        if (s_currentLives <= 0)
        {
            s_currentLives = 0;
            Die();
            return;
        }

        s_isInvulnerable = true;
        s_invulnerabilityTimer = s_invulnerabilityDuration;
    }

    public static void TakeDamage() { TakeDamage(1); }

    public static void Heal()
    {
        if (s_isDead) return;

        if (s_currentLives < s_maxLives)
        {
            s_currentLives++;
            Debug.Log("[PlayerHealth] curado. Vidas: " + s_currentLives);
            OnHealthChanged?.Invoke(s_currentLives, s_maxLives);
        }
    }

    private static void Die()
    {
        s_isDead = true;

        Debug.Log("[PlayerHealth] GAME OVER");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                if (ph.subLevelController != null)
                    ph.subLevelController.enabled = false;
                else if (ph.playerMovement != null)
                    ph.playerMovement.enabled = false;
            }
        }

        OnPlayerDeath?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    public static void ResetDeath()
    {
        s_isDead = false;
        s_isInvulnerable = false;
        s_invulnerabilityTimer = 0f;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                if (ph.subLevelController != null)
                    ph.subLevelController.enabled = true;
                else if (ph.playerMovement != null)
                    ph.playerMovement.enabled = true;
            }
        }
    }

    public static void ResetForNewScene()
    {
        s_currentLives = s_maxLives;
        s_isDead = false;
        s_isInvulnerable = false;
        s_invulnerabilityTimer = 0f;
        OnHealthChanged?.Invoke(s_currentLives, s_maxLives);
    }

    public static void RestoreFullHealth()
    {
        s_currentLives = s_maxLives;
        s_isDead = false;
        s_isInvulnerable = false;
        s_invulnerabilityTimer = 0f;
        OnHealthChanged?.Invoke(s_currentLives, s_maxLives);
    }

    public static int GetCurrentLives() => s_currentLives;
    public static int GetMaxLives() => s_maxLives;
    public static bool IsInvulnerable() => s_isInvulnerable;
    public static bool IsDead() => s_isDead;
}
