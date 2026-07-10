using UnityEngine;

/// <summary>
/// JacketPickup - Power-up de chaqueta que otorga teletransporte hacia adelante.
/// Aparece sobre cualquier cable.
/// Cuando el jugador la recoge, obtiene el poder de teletransportarse.
/// La chaqueta reaparece despues de un tiempo.
/// </summary>
public class JacketPickup : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float tiempoReaparicion = 10f;
    [SerializeField] private float alturaFlotacion = 1.5f;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Cables (posiciones X)")]
    [SerializeField] private float[] cableXPositions = { -3f, 0f, 3f };
    [SerializeField] private float rangoZMin = 20f;
    [SerializeField] private float rangoZMax = 100f;

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

        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null) jugador = playerObj.transform;
        }
    }

    private void Update()
    {
        // Rotar la chaqueta
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);

        // Flotar arriba y abajo
        float offsetY = Mathf.Sin(Time.time * 2f) * 0.2f;
        transform.position = new Vector3(
            posicionOriginal.x,
            posicionOriginal.y + offsetY,
            posicionOriginal.z
        );

        // Reaparecer despues de un tiempo
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

        PlayerCableMovement pcm = other.GetComponent<PlayerCableMovement>();
        if (pcm != null && !pcm.TienePoder())
        {
            pcm.ActivarPoder();
            recogida = true;
            timerReaparicion = tiempoReaparicion;

            if (rend != null) rend.enabled = false;
            if (col != null) col.enabled = false;

            Debug.Log("JacketPickup: Poder de teletransporte recogido.");
        }
    }

    private void Reposicionar()
    {
        // Elegir cable aleatorio
        int indiceCable = Random.Range(0, cableXPositions.Length);
        float x = cableXPositions[indiceCable];
        float z = Random.Range(rangoZMin, rangoZMax);

        posicionOriginal = new Vector3(x, alturaFlotacion, z);
        transform.position = posicionOriginal;
    }
}
