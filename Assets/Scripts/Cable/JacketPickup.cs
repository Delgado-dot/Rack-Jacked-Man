using UnityEngine;

public class JacketPickup : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float tiempoReaparicion = 10f;
    [SerializeField] private float alturaFlotacion = 1.5f;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -2.5f, 0f, 2.5f };
    [SerializeField] private float rangoZMin = 20f;
    [SerializeField] private float rangoZMax = 100f;

    [Header("Tamano")]
    [SerializeField] private float escalaInicial = 1.2f;

    [Header("Referencia")]
    [SerializeField] private Transform jugador;

    private float timerReaparicion = 0f;
    private Vector3 posicionOriginal;
    private Renderer rend;
    private Collider col;
    private bool recogida = false;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        posicionOriginal = transform.position;

        transform.localScale = Vector3.one * escalaInicial;

        if (rend != null)
        {
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rend.material.color = Color.yellow;
            rend.material.SetColor("_EmissionColor", Color.yellow * 0.5f);
            rend.material.EnableKeyword("_EMISSION");
        }

        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null) jugador = playerObj.transform;
        }
    }

    private void Update()
    {
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
                if (rend != null) rend.enabled = true;
                if (col != null) col.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (recogida) return;

        SubLevelPlayerController slpc = other.GetComponent<SubLevelPlayerController>();
        if (slpc != null && !slpc.TienePoder())
        {
            slpc.ActivarPoder();
            recogida = true;
            timerReaparicion = tiempoReaparicion;

            if (rend != null) rend.enabled = false;
            if (col != null) col.enabled = false;

            Debug.Log("JacketPickup: Poder de teletransporte recogido.");
        }
    }

    private void Reposicionar()
    {
        int indiceCable = Random.Range(0, cableXPositions.Length);
        float x = cableXPositions[indiceCable];
        float z = jugador.position.z + Random.Range(rangoZMin, rangoZMax);

        posicionOriginal = new Vector3(x, alturaFlotacion, z);
        transform.position = posicionOriginal;
    }
}
