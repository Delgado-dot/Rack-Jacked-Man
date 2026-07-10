using UnityEngine;

public class EnemyCable : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Tamano")]
    [SerializeField] private float escalaInicial = 1.5f;

    [Header("Danio")]
    [SerializeField] private float danioCooldown = 1f;
    private float timerDanio = 0f;

    private Vector3 posicionInicial;
    private SubLevelPlayerController playerController;
    private bool jugadorEnRango = false;

    private void Start()
    {
        posicionInicial = transform.position;
        transform.localScale = Vector3.one * escalaInicial;
    }

    private void Update()
    {
        transform.position += Vector3.back * velocidad * Time.deltaTime;

        float distancia = Vector3.Distance(transform.position, posicionInicial);
        if (distancia > distanciaMaxima)
        {
            Destroy(gameObject);
            return;
        }

        if (jugadorEnRango && playerController != null)
        {
            timerDanio -= Time.deltaTime;
            if (timerDanio <= 0f)
            {
                playerController.TakeDamage();
                timerDanio = danioCooldown;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnRango = true;
        playerController = other.GetComponent<SubLevelPlayerController>();
        timerDanio = 0f;

        if (playerController != null)
        {
            playerController.TakeDamage();
            timerDanio = danioCooldown;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnRango = false;
        playerController = null;
    }
}
