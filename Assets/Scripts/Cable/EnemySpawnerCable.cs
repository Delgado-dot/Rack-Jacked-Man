using UnityEngine;

public class EnemySpawnerCable : MonoBehaviour
{
    [Header("Prefab del enemigo (opcional)")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -2.5f, 0f, 2.5f };
    [SerializeField] private float alturaSpawn = 1f;

    [Header("Referencia al jugador")]
    [SerializeField] private Transform jugador;

    [Header("Configuracion")]
    [SerializeField] private float tiempoEntreEnemigos = 2f;
    [SerializeField] private float distanciaAparicion = 40f;
    [SerializeField] private float velocidadEnemigo = 5f;
    [SerializeField] private float distanciaMaxima = 60f;

    [Header("Tamano del enemigo")]
    [SerializeField] private float enemigoEscala = 1.5f;

    [Header("Rango de variacion")]
    [SerializeField] private float variacionX = 0.3f;

    [Header("Cantidad constante de enemigos")]
    [SerializeField] private int maxEnemigosActivos = 5;

    private float spawnTimer;
    private int enemigosActivos = 0;

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
        if (jugador == null) return;

        enemigosActivos = GameObject.FindGameObjectsWithTag("Enemy").Length;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && enemigosActivos < maxEnemigosActivos)
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
        int indiceCable = Random.Range(0, cableXPositions.Length);
        float xCable = cableXPositions[indiceCable];

        Vector3 spawnPos = new Vector3(
            xCable + Random.Range(-variacionX, variacionX),
            alturaSpawn,
            jugador.position.z + distanciaAparicion
        );

        GameObject enemigo;

        if (enemyPrefab != null)
        {
            enemigo = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            enemigo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            enemigo.transform.position = spawnPos;
            enemigo.transform.localScale = new Vector3(0.8f, 1f, 0.8f) * enemigoEscala;
            enemigo.tag = "Enemy";

            Renderer rend = enemigo.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                rend.material.color = new Color(0.8f, 0.2f, 0.2f);
            }

            Collider col = enemigo.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        EnemyCable ec = enemigo.GetComponent<EnemyCable>();
        if (ec == null)
        {
            ec = enemigo.AddComponent<EnemyCable>();
        }
    }
}
