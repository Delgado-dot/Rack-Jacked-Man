using UnityEngine;

/// <summary>
/// EnemyBug - Enemigo que persigue constantemente al jugador.
/// Copia el comportamiento del EnemyBug original del juego en Python/Pygame.
/// No patrulla, no tiene estados, simplemente persigue al jugador sin descanso.
/// </summary>
public class EnemyBug : MonoBehaviour
{
    [Header("Configuracion de movimiento")]
    [SerializeField] private float speed = 3f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Deteccion de colision")]
    [SerializeField] private float collisionDistance = 1.2f;

    [Header("Muerte por pisoton")]
    [SerializeField] private float stompThreshold = -0.5f;
    [SerializeField] private float stompAngleThreshold = 0.3f;

    private CharacterController playerController;

    private void Start()
    {
        // Buscar al jugador automaticamente si no se asigno manualmente
        // Primero intenta por tag "Player", si no lo encuentra busca por nombre "Player"
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
            }
            else
            {
                Debug.LogWarning("EnemyBug: No se encontro el jugador. El enemigo no podra perseguir.");
            }
        }
        else
        {
            playerController = player.GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // === PERSECUCION CONSTANTE ===
        // Calcular direccion hacia el jugador (X, Y y Z)
        Vector3 direction = (player.position - transform.position).normalized;

        // Moverse hacia el jugador usando Time.deltaTime
        transform.position += direction * speed * Time.deltaTime;

        // === ROTACION MIRANDO AL JUGADOR ===
        Vector3 lookDirection = player.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // === DETECCION DE COLISION ===
        // Verificar distancia entre enemigo y jugador
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < collisionDistance)
        {
            // === MUERTE POR PISOTON ===
            // Verificar si el jugador esta cayendo y esta por encima del enemigo
            bool isFalling = false;
            if (playerController != null)
            {
                isFalling = playerController.velocity.y < stompThreshold;
            }

            // Verificar angulo: el jugador debe estar significativamente por encima
            Vector3 toPlayer = player.position - transform.position;
            bool isAbove = toPlayer.y > stompAngleThreshold;

            if (isFalling && isAbove)
            {
                // El jugador piso al enemigo: destruir este objeto
                Destroy(gameObject);
                return;
            }

            // === DANNO AL JUGADOR ===
            // TODO: Llamar a PlayerHealth.TakeDamage() cuando se implemente el sistema de vida del jugador.
            // Por ahora solo se registra en consola.
            Debug.Log("EnemyBug impacto al jugador. Aqui se llamaria a PlayerHealth.TakeDamage().");
        }
    }

    // Visualizar radio de colision en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
    }
}
