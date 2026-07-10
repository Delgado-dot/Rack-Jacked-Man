using UnityEngine;

public class EnemyCable : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Danio")]
    [SerializeField] private int danoAlContacto = 1;

    private Vector3 posicionInicial;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        transform.position += Vector3.back * velocidad * Time.deltaTime;

        float distancia = Vector3.Distance(transform.position, posicionInicial);
        if (distancia > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SubLevelPlayerController playerController = other.GetComponent<SubLevelPlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(danoAlContacto);
        }

        Destroy(gameObject);
    }
}
