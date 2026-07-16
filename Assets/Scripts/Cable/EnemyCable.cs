using UnityEngine;

public class EnemyCable : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Danio")]
    [SerializeField] private int danoAlContacto = 1;

    private Vector3 posicionInicial;
    private bool destruido = false;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        transform.position += Vector3.forward * velocidad * Time.deltaTime;

        if (Vector3.Distance(transform.position, posicionInicial) > distanciaMaxima)
        {
            Destruir();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth.TakeDamage(danoAlContacto);
        Destruir();
    }

    public void TakeDamage(int amount)
    {
        Destruir();
    }

    private void Destruir()
    {
        if (destruido) return;
        destruido = true;
        EnemySpawnerCable.DecrementarEnemigos();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!destruido)
        {
            destruido = true;
            EnemySpawnerCable.DecrementarEnemigos();
        }
    }
}
