using UnityEngine;

/// <summary>
/// EnemyBug - Enemigo que persigue constantemente al jugador.
/// Se mueve libremente por el mapa usando CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnemyBug : MonoBehaviour
{
    [Header("Configuracion de movimiento")]
    [SerializeField] private float speed = 3f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

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

    private void Start()
    {
        enemyController = GetComponent<CharacterController>();

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
    }

    private void Update()
    {
        if (player == null) return;

        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }

        // Mantener en el suelo
        if (enemyController.isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }

        // Direccion hacia el jugador (X y Z, sin importar la altura)
        Vector3 toPlayer = player.position - transform.position;
        Vector3 direction = new Vector3(toPlayer.x, 0, toPlayer.z).normalized;

        // Moverse hacia el jugador
        enemyController.Move(direction * speed * Time.deltaTime);

        // Rotar mirando al jugador
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        enemyController.Move(velocity * Time.deltaTime);

        // Deteccion de colision
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < collisionDistance)
        {
            // Muerte por pisoton
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
                return;
            }

            // Danio al jugador
            if (playerHealth != null && damageTimer <= 0f)
            {
                playerHealth.TakeDamage();
                damageTimer = damageCooldown;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
    }
}
