using UnityEngine;

/// <summary>
/// CableHazard - Cable que puede estar en estado NORMAL o ELECTRIFICADO.
/// Cuando esta electrificado, cambia material, activa particulas y sonido,
/// y dania al jugador al tocarlo.
/// </summary>
public class CableHazard : MonoBehaviour
{
    public enum EstadoCable { NORMAL, ELECTRIFICADO }

    [Header("Estado")]
    [SerializeField] private EstadoCable estadoActual = EstadoCable.NORMAL;

    [Header("Materiales")]
    [SerializeField] private Material materialNormal;
    [SerializeField] private Material materialElectrificado;

    [Header("Efectos")]
    [SerializeField] private ParticleSystem particulasElectricas;
    [SerializeField] private AudioSource sonidoElectrico;

    [Header("Danio")]
    [SerializeField] private float danioIntervalo = 0.5f;
    private float danioTimer = 0f;

    [Header("References")]
    private Renderer cableRenderer;
    private bool jugadorEnCable = false;

    private void Awake()
    {
        cableRenderer = GetComponent<Renderer>();
        if (cableRenderer == null)
        {
            cableRenderer = GetComponentInChildren<Renderer>();
        }
    }

    private void Start()
    {
        AplicarEstado(estadoActual);
    }

    private void Update()
    {
        if (jugadorEnCable && estadoActual == EstadoCable.ELECTRIFICADO)
        {
            danioTimer -= Time.deltaTime;
            if (danioTimer <= 0f)
            {
                PlayerHealth.TakeDamage();
                danioTimer = danioIntervalo;
            }
        }
    }

    /// <summary>
    /// Cambiar estado del cable desde el Inspector o desde otro script.
    /// </summary>
    public void SetEstado(EstadoCable nuevoEstado)
    {
        estadoActual = nuevoEstado;
        AplicarEstado(estadoActual);
    }

    /// <summary>
    /// Alternar entre NORMAL y ELECTRIFICADO.
    /// </summary>
    public void ToggleEstado()
    {
        if (estadoActual == EstadoCable.NORMAL)
            SetEstado(EstadoCable.ELECTRIFICADO);
        else
            SetEstado(EstadoCable.NORMAL);
    }

    public EstadoCable GetEstado()
    {
        return estadoActual;
    }

    private void AplicarEstado(EstadoCable estado)
    {
        switch (estado)
        {
            case EstadoCable.NORMAL:
                if (cableRenderer != null && materialNormal != null)
                {
                    cableRenderer.sharedMaterial = materialNormal;
                    if (cableRenderer.material.HasProperty("_EmissionColor"))
                    {
                        cableRenderer.material.EnableKeyword("_EMISSION");
                        cableRenderer.material.SetColor("_EmissionColor", Color.black);
                    }
                }
                if (particulasElectricas != null) particulasElectricas.Stop();
                if (sonidoElectrico != null) sonidoElectrico.Stop();
                break;

            case EstadoCable.ELECTRIFICADO:
                if (cableRenderer != null && materialElectrificado != null)
                {
                    cableRenderer.sharedMaterial = materialElectrificado;
                    if (cableRenderer.material.HasProperty("_EmissionColor"))
                    {
                        cableRenderer.material.EnableKeyword("_EMISSION");
                        cableRenderer.material.SetColor("_EmissionColor", Color.cyan * 2f);
                    }
                }
                if (particulasElectricas != null) particulasElectricas.Play();
                if (sonidoElectrico != null) sonidoElectrico.Play();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnCable = true;
            danioTimer = 0f;

            if (estadoActual == EstadoCable.ELECTRIFICADO)
            {
                PlayerHealth.TakeDamage();
                danioTimer = danioIntervalo;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnCable = false;
        }
    }
}
