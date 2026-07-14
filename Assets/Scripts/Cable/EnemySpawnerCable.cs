using UnityEngine;

public class EnemySpawnerCable : MonoBehaviour
{
    [Header("Prefab del enemigo (recomendado)")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Fallback: si no hay prefab, crea un cilindro")]
    [SerializeField] private float enemigoEscala = 1.5f;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -2.5f, 0f, 2.5f };
    [SerializeField] private float alturaSpawn = 1f;

    [Header("Referencia al jugador")]
    [SerializeField] private Transform jugador;

    [Header("Configuracion")]
    [SerializeField] private float tiempoEntreEnemigos = 2f;
    [SerializeField] private float distanciaAparicion = 40f;

    [Header("Rango de variacion")]
    [SerializeField] private float variacionX = 0.3f;

    [Header("Cantidad constante de enemigos")]
    [SerializeField] private int maxEnemigosActivos = 5;

    private float spawnTimer;
    private static int enemigosActivos = 0;

    private void Start()
    {
        spawnTimer = tiempoEntreEnemigos;

        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) jugador = playerObj.transform;
        }
    }

    private void Update()
    {
        if (jugador == null) return;

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

    public static void DecrementarEnemigos()
    {
        enemigosActivos = Mathf.Max(0, enemigosActivos - 1);
    }

    public static int GetEnemigosActivos()
    {
        return enemigosActivos;
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
            enemigo = CreateFallbackEnemy(spawnPos);
        }

        if (enemigo.GetComponent<EnemyCable>() == null)
        {
            enemigo.AddComponent<EnemyCable>();
        }

        enemigosActivos++;
    }

    private GameObject CreateFallbackEnemy(Vector3 position)
    {
        GameObject enemigo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        enemigo.transform.position = position;
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

        return enemigo;
    }
}
