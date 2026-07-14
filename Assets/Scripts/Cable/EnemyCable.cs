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
        transform.position += Vector3.forward * velocidad * Time.deltaTime;

        float distancia = Vector3.Distance(transform.position, posicionInicial);
        if (distancia > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private bool destroyed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(danoAlContacto);

        DestroyEnemy();
    }

    public void TakeDamage(int amount)
    {
        DestroyEnemy();
    }

    private void DestroyEnemy()
    {
        if (destroyed) return;
        destroyed = true;
        EnemySpawnerCable.DecrementarEnemigos();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!destroyed)
        {
            destroyed = true;
            EnemySpawnerCable.DecrementarEnemigos();
        }
    }
}
