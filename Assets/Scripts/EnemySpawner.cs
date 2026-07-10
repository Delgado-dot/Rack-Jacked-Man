using UnityEngine;

/// <summary>
/// EnemySpawner - Genera enemigos en cables aleatorios.
/// Los enemigos aparecen delante del jugador y avanzan en linea recta.
/// NO persiguen. NO usan NavMesh.
/// Se destruyen al salir del escenario.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab del enemigo")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Cables")]
    [SerializeField] private Transform[] cables;
    [SerializeField] private Transform jugador;

    [Header("Configuracion de spawn")]
    [SerializeField] private float tiempoEntreEnemigos = 3f;
    [SerializeField] private float distanciaAparicion = 30f;
    [SerializeField] private float velocidadEnemigo = 5f;
    [SerializeField] private float limiteDestruccion = 50f;

    [Header("Estado")]
    [SerializeField] private bool spawnerActivo = true;

    private float spawnTimer = 0f;

    private void Start()
    {
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null) jugador = playerObj.transform;
        }

        spawnTimer = tiempoEntreEnemigos;
    }

    private void Update()
    {
        if (!spawnerActivo || enemyPrefab == null || jugador == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemigo();
            spawnTimer = tiempoEntreEnemigos;
        }
    }

    private void SpawnEnemigo()
    {
        if (cables == null || cables.Length == 0) return;

        // Elegir cable aleatorio
        int indiceCable = Random.Range(0, cables.Length);
        Transform cableElegido = cables[indiceCable];
        if (cableElegido == null) return;

        // Calcular posicion de spawn delante del jugador
        Vector3 spawnPos = jugador.position + jugador.forward * distanciaAparicion;

        // Ajustar X a la posicion del cable
        spawnPos.x = cableElegido.position.x;

        // Mantener Y del cable
        spawnPos.y = cableElegido.position.y + 1.5f;

        spawnPos.z = 0f;

        // Instanciar enemigo
        GameObject enemigo = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Configurar movimiento forward
        ForwardEnemy fe = enemigo.GetComponent<ForwardEnemy>();
        if (fe != null)
        {
            fe.Inicializar(velocidadEnemigo, limiteDestruccion, jugador);
        }
        else
        {
            // Si no tiene ForwardEnemy, agregarlo
            fe = enemigo.AddComponent<ForwardEnemy>();
            fe.Inicializar(velocidadEnemigo, limiteDestruccion, jugador);
        }
    }
}

/// <summary>
/// ForwardEnemy - Enemigo que avanza en linea recta hacia el jugador.
/// NO persigue. NO gira. Solo avanza hacia adelante.
/// Se destruye al pasar cierta distancia.
/// </summary>
public class ForwardEnemy : MonoBehaviour
{
    private float velocidad = 5f;
    private float limiteDestruccion = 50f;
    private Transform jugador;
    private Vector3 direccion;
    private float distanciaRecorrida = 0f;

    public void Inicializar(float vel, float limite, Transform player)
    {
        velocidad = vel;
        limiteDestruccion = limite;
        jugador = player;

        if (jugador != null)
        {
            direccion = (jugador.position - transform.position).normalized;
            direccion.y = 0f;
            direccion.Normalize();
        }
        else
        {
            direccion = transform.forward;
        }
    }

    private void Update()
    {
        // Avanzar en linea recta
        Vector3 movimiento = direccion * velocidad * Time.deltaTime;
        transform.position += movimiento;
        distanciaRecorrida += movimiento.magnitude;

        // Mirar en la direccion del movimiento
        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }

        // Destruir si paso el limite
        if (distanciaRecorrida > limiteDestruccion)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Destruir si sale del escenario (por trigger boundary)
        if (other.CompareTag("Finish"))
        {
            Destroy(gameObject);
        }
    }
}
