using UnityEngine;

public class EnemySpawnerCable : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Fallback (si no hay prefab)")]
    [SerializeField] private float enemigoEscala = 1.5f;
    [SerializeField] private float enemigoAnchoX = 2f;
    [SerializeField] private float enemigoLargoZ = 4f;

    [Header("Cables de spawn (solo 3)")]
    [SerializeField] private float[] cableXPositions = { -2.3f, 0f, 2.3f };
    [SerializeField] private float alturaSpawn = 1f;

    [Header("Referencia al jugador")]
    [SerializeField] private Transform jugador;

    [Header("Configuracion")]
    [SerializeField] private float tiempoEntreEnemigos = 2f;
    [SerializeField] private float distanciaAparicion = 40f;
    [SerializeField] private float variacionX = 0f;
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

    private void SpawnEnemigo()
    {
        int indice = Random.Range(0, cableXPositions.Length);
        float xCable = cableXPositions[indice] + Random.Range(-variacionX, variacionX);

        Vector3 spawnPos = new Vector3(
            xCable,
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
            enemigo = CrearFallback(spawnPos);
        }

        if (enemigo.GetComponent<EnemyCable>() == null)
            enemigo.AddComponent<EnemyCable>();

        enemigosActivos++;
    }

    private GameObject CrearFallback(Vector3 position)
    {
        GameObject enemigo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        enemigo.transform.position = position;
        enemigo.transform.localScale = new Vector3(enemigoAnchoX, enemigoEscala, enemigoLargoZ);
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

    public static void DecrementarEnemigos()
    {
        enemigosActivos = Mathf.Max(0, enemigosActivos - 1);
    }
}
