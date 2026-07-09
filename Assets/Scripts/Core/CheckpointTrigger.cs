using UnityEngine;

/// <summary>
/// CheckpointTrigger - Trigger que registra un checkpoint o completa el nivel.
/// Se usa en RackStart (checkpoint inicial), checkpoints intermedios, y RackGoal.
/// Usa OnTriggerEnter Y verificacion por distancia como fallback
/// (el jugador usa CharacterController, no Rigidbody).
/// </summary>
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private bool isGoal = false;

    [Header("Fallback por distancia (para CharacterController)")]
    [SerializeField] private float checkDistance = 1.5f;
    [SerializeField] private float cooldown = 1f;

    private Transform playerTransform;
    private float cooldownTimer = 0f;

    private void Start()
    {
        // Buscar jugador para el fallback por distancia
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name == "Player")
        {
            ActivateCheckpoint();
        }
    }

    private void Update()
    {
        // Reducir cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Fallback: verificar distancia manualmente cada frame
        if (playerTransform == null) return;

        float distance = Vector3.Distance(
            transform.position,
            playerTransform.position
        );

        if (distance < checkDistance)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        if (cooldownTimer > 0f) return;
        if (GameManager.Instance == null) return;

        cooldownTimer = cooldown;

        if (isGoal)
        {
            GameManager.Instance.LevelCompleted();
        }
        else
        {
            GameManager.Instance.RegisterCheckpoint(transform);
        }
    }
}
