using UnityEngine;

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

    [Header("Cooldown de danio")]
    [SerializeField] private float damageCooldown = 1.5f;

    [Header("Muerte por pisoton")]
    [SerializeField] private float stompThreshold = -0.5f;
    [SerializeField] private float stompAngleThreshold = 0.3f;

    [Header("Evitar obstaculos")]
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float stuckTimeThreshold = 1f;

    private CharacterController enemyController;
    private CharacterController playerController;
    private PlayerHealth playerHealth;
    private Vector3 velocity;
    private float gravity = -20f;
    private float damageTimer = 0f;

    private Vector3 patrolTarget;
    private float patrolTimer = 0f;
    private Vector3 startPosition;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    private void Start()
    {
        enemyController = GetComponent<CharacterController>();
        enemyController.height = 0.8f;
        enemyController.radius = 0.3f;
        enemyController.center = new Vector3(0, 0.4f, 0);

        startPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
                playerObj = GameObject.Find("Player");

            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<CharacterController>();
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
        else
        {
            playerController = player.GetComponent<CharacterController>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        SetNewPatrolTarget();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        if (enemyController.isGrounded && velocity.y < 0)
            velocity.y = -5f;

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

        CheckStuck();
        CheckStomp();
    }

    private bool IsObstacleAhead(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        return Physics.Raycast(origin, direction, obstacleCheckDistance);
    }

    private Vector3 GetAvoidanceDirection(Vector3 desiredDirection)
    {
        if (!IsObstacleAhead(desiredDirection))
            return desiredDirection;

        Vector3 right = Quaternion.Euler(0, 45, 0) * desiredDirection;
        Vector3 left = Quaternion.Euler(0, -45, 0) * desiredDirection;

        if (!IsObstacleAhead(right)) return right;
        if (!IsObstacleAhead(left)) return left;

        Vector3 farRight = Quaternion.Euler(0, 90, 0) * desiredDirection;
        Vector3 farLeft = Quaternion.Euler(0, -90, 0) * desiredDirection;

        if (!IsObstacleAhead(farRight)) return farRight;
        if (!IsObstacleAhead(farLeft)) return farLeft;

        return -desiredDirection;
    }

    private void CheckStuck()
    {
        float distMoved = Vector3.Distance(transform.position, lastPosition);

        if (distMoved < 0.05f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckTimeThreshold)
            {
                SetNewPatrolTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
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
                SetNewPatrolTarget();
        }
        else
        {
            direction = GetAvoidanceDirection(direction);
            enemyController.Move(direction * speed * 0.5f * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
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

        direction = GetAvoidanceDirection(direction);
        enemyController.Move(direction * speed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
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
        if (distance > enemyController.height) return;

        bool isFalling = false;
        if (playerController != null)
            isFalling = playerController.velocity.y < stompThreshold;

        Vector3 toPlayer3D = player.position - transform.position;
        bool isAbove = toPlayer3D.y > stompAngleThreshold;

        if (isFalling && isAbove)
            Destroy(gameObject);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            if (damageTimer <= 0f)
            {
                PlayerHealth ph = hit.gameObject.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    PlayerHealth.TakeDamage();
                    damageTimer = damageCooldown;
                    Debug.Log("EnemyBug: Danio al jugador por contacto fisico!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Vector3 center = Application.isPlaying ? transform.position + Vector3.up * 0.4f : transform.position;
        Gizmos.DrawWireSphere(center, 0.3f);
        Gizmos.DrawWireSphere(center + Vector3.up * 0.8f, 0.3f);
    }
}
