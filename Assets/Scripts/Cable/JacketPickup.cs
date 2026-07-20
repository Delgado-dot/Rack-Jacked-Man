using UnityEngine;

public class JacketPickup : MonoBehaviour
{
    [Header("Prefab del powerup (recomendado)")]
    [SerializeField] private GameObject pickupPrefab;

    [Header("Modelo visual")]
    [SerializeField] private GameObject modeloVisual;

    [Header("Configuracion")]
    [SerializeField] private float tiempoReaparicion = 10f;
    [SerializeField] private float alturaFlotacion = 1.5f;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -2.5f, 0f, 2.5f };
    [SerializeField] private float rangoZMin = 20f;
    [SerializeField] private float rangoZMax = 100f;

    [Header("Referencia")]
    [SerializeField] private Transform jugador;

    private float timerReaparicion = 0f;
    private Vector3 posicionOriginal;
    private Collider col;
    private bool recogida = false;

    private void Start()
    {
        col = GetComponent<Collider>();
        posicionOriginal = transform.position;

        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                jugador = playerObj.transform;
        }
    }

    private void Update()
    {
        // Girar y flotar el pickup completo
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);

        float offsetY = Mathf.Sin(Time.time * 2f) * 0.2f;

        transform.position = new Vector3(
            posicionOriginal.x,
            posicionOriginal.y + offsetY,
            posicionOriginal.z
        );

        if (recogida)
        {
            timerReaparicion -= Time.deltaTime;

            if (timerReaparicion <= 0f)
            {
                Reposicionar();

                recogida = false;

                if (modeloVisual != null)
                    modeloVisual.SetActive(true);

                if (col != null)
                    col.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (recogida)
            return;

        SubLevelPlayerController slpc = other.GetComponent<SubLevelPlayerController>();

        if (slpc != null && !slpc.TienePoder())
        {
            slpc.ActivarPoder();

            recogida = true;
            timerReaparicion = tiempoReaparicion;

            if (modeloVisual != null)
                modeloVisual.SetActive(false);

            if (col != null)
                col.enabled = false;

            Debug.Log("JacketPickup: Poder de teletransporte recogido.");
        }
    }

    private void Reposicionar()
    {
        int indiceCable = Random.Range(0, cableXPositions.Length);

        float x = cableXPositions[indiceCable];
        float z = jugador.position.z + Random.Range(rangoZMin, rangoZMax);

        posicionOriginal = new Vector3(
            x,
            alturaFlotacion,
            z
        );

        transform.position = posicionOriginal;
    }
}