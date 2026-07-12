using UnityEngine;

/// <summary>
/// EnemyBug - Enemigo con estados: Patrullar y Persecucion.
/// Patrulla por el piso actual. Al detectar jugador, lo persigue.
/// El jugador puede matarlo saltando encima.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnemyBug : MonoBehaviour
{
    public enum Estado { Patrullar, Persecucion }

    [Header("Estado actual")]
    [SerializeField] private Estado estadoActual = Estado.Patrullar;

    [Header("Configuracion de movimiento")]
    [SerializeField] private float speed = 3f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Deteccion")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float lostRange = 20f;

    [Header("Patrulla")]
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Deteccion de colision")]
    [SerializeField] private float collisionDistance = 1.2f;

    [Header("Cooldown de danio")]
    [SerializeField] private float damageCooldown = 1.5f;

    [Header("Muerte por pisoton")]
    [SerializeField] private float stompThreshold = -0.5f;
    [SerializeField] private float stompAngleThreshold = 0.3f;

    private CharacterController enemyController;
    private CharacterController playerController;
    private PlayerHealth playerHealth;
    private Vector3 velocity;
    private float gravity = -20f;
    private float damageTimer = 0f;

    private Vector3 patrolTarget;
    private float patrolTimer = 0f;
    private Vector3 startPosition;

    private void Start()
    {
        enemyController = GetComponent<CharacterController>();
        startPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }

            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<CharacterController>();
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
            else
            {
                Debug.LogWarning("EnemyBug: No se encontro el jugador.");
            }
        }
        else
        {
            playerController = player.GetComponent<CharacterController>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }

        if (enemyController.isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }

        switch (estadoActual)
        {
            case Estado.Patrullar:
                UpdatePatrullar();
                break;
            case Estado.Persecucion:
                UpdatePersecucion();
                break;
        }

        velocity.y += gravity * Time.deltaTime;
        enemyController.Move(velocity * Time.deltaTime);

        CheckStomp();
        CheckDamage();
    }

    private void UpdatePatrullar()
    {
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= detectionRange)
            {
                estadoActual = Estado.Persecucion;
                return;
            }
        }

        Vector3 toTarget = patrolTarget - transform.position;
        Vector3 direction = new Vector3(toTarget.x, 0, toTarget.z).normalized;

        if (toTarget.magnitude < 0.5f)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                SetNewPatrolTarget();
            }
        }
        else
        {
            enemyController.Move(direction * speed * 0.5f * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    5f * Time.deltaTime
                );
            }
        }
    }

    private void UpdatePersecucion()
    {
        if (player == null)
        {
            estadoActual = Estado.Patrullar;
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > lostRange)
        {
            estadoActual = Estado.Patrullar;
            SetNewPatrolTarget();
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        Vector3 direction = new Vector3(toPlayer.x, 0, toPlayer.z).normalized;

        enemyController.Move(direction * speed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector3 randomOffset = Random.insideUnitSphere * patrolRadius;
        randomOffset.y = 0;
        patrolTarget = startPosition + randomOffset;
        patrolTimer = patrolWaitTime;
    }

    private void CheckStomp()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > collisionDistance) return;

        bool isFalling = false;
        if (playerController != null)
        {
            isFalling = playerController.velocity.y < stompThreshold;
        }

        Vector3 toPlayer3D = player.position - transform.position;
        bool isAbove = toPlayer3D.y > stompAngleThreshold;

        if (isFalling && isAbove)
        {
            Destroy(gameObject);
        }
    }

    private void CheckDamage()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > collisionDistance) return;

        if (playerHealth != null && damageTimer <= 0f)
        {
            playerHealth.TakeDamage();
            damageTimer = damageCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);

        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }
}
