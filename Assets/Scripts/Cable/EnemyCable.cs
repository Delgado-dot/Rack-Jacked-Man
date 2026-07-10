using UnityEngine;

/// <summary>
/// EnemyCable - Enemigo que avanza en linea recta hacia el jugador.
/// NO persigue. NO gira. Solo avanza hacia adelante.
/// Se destruye al llegar al final del recorrido.
/// </summary>
public class EnemyCable : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Danio")]
    [SerializeField] private float danioCooldown = 1f;
    private float timerDanio = 0f;

    private Vector3 posicionInicial;
    private PlayerHealth playerHealth;
    private bool jugadorEnRango = false;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        // Avanzar en linea recta (hacia -Z, hacia el jugador)
        transform.position += Vector3.back * velocidad * Time.deltaTime;

        // Destruir si recorrio la distancia maxima
        float distancia = Vector3.Distance(transform.position, posicionInicial);
        if (distancia > distanciaMaxima)
        {
            Destroy(gameObject);
            return;
        }

        // Danio por contacto
        if (jugadorEnRango && playerHealth != null)
        {
            timerDanio -= Time.deltaTime;
            if (timerDanio <= 0f)
            {
                playerHealth.TakeDamage();
                timerDanio = danioCooldown;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnRango = true;
        playerHealth = other.GetComponent<PlayerHealth>();
        timerDanio = 0f;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
            timerDanio = danioCooldown;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnRango = false;
        playerHealth = null;
    }
}
