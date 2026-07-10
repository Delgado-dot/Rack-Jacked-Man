using UnityEngine;

/// <summary>
/// EnemySpawnerCable - Genera enemigos aleatoriamente sobre los cables.
/// Los enemigos aparecen delante del jugador y avanzan en linea recta.
/// NO persiguen. NO usan NavMesh.
/// Se destruyen al llegar al final.
/// </summary>
public class EnemySpawnerCable : MonoBehaviour
{
    [Header("Prefab del enemigo")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -3f, 0f, 3f };
    [SerializeField] private float alturaSpawn = 1f;

    [Header("Referencia al jugador")]
    [SerializeField] private Transform jugador;

    [Header("Configuracion")]
    [SerializeField] private float tiempoEntreEnemigos = 2f;
    [SerializeField] private float distanciaAparicion = 40f;
    [SerializeField] private float velocidadEnemigo = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Rango de variacion")]
    [SerializeField] private float variacionX = 0.3f;

    private float spawnTimer;

    private void Start()
    {
        spawnTimer = tiempoEntreEnemigos;

        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null) jugador = playerObj.transform;
        }
    }

    private void Update()
    {
        if (enemyPrefab == null || jugador == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemigo();
            spawnTimer = tiempoEntreEnemigos;
        }
    }

    public void ReducirIntervalo(float cantidad)
    {
        tiempoEntreEnemigos = Mathf.Max(0.5f, tiempoEntreEnemigos - cantidad);
    }

    private void SpawnEnemigo()
    {
        // Elegir cable aleatorio
        int indiceCable = Random.Range(0, cableXPositions.Length);
        float xCable = cableXPositions[indiceCable];

        // Posicion de spawn delante del jugador
        Vector3 spawnPos = new Vector3(
            xCable + Random.Range(-variacionX, variacionX),
            alturaSpawn,
            jugador.position.z + distanciaAparicion
        );

        // Instanciar enemigo
        GameObject enemigo = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Configurar EnemyCable
        EnemyCable ec = enemigo.GetComponent<EnemyCable>();
        if (ec == null)
        {
            ec = enemigo.AddComponent<EnemyCable>();
        }
    }
}
