using UnityEngine;

public class CableSegment : MonoBehaviour
{
    public enum Estado { NORMAL, ELECTRIFICADO }

    [Header("Estado")]
    [SerializeField] private Estado estadoActual = Estado.NORMAL;

    [Header("Materiales")]
    [SerializeField] private Material materialNormal;
    [SerializeField] private Material materialElectrificado;

    [Header("Efectos")]
    [SerializeField] private ParticleSystem particulas;
    [SerializeField] private AudioSource audioElectrico;

    [Header("Danio")]
    [SerializeField] private float danioCooldown = 0.5f;
    private float timerDanio = 0f;

    private Renderer rend;
    private bool jugadorEnZona = false;
    private SubLevelPlayerController playerController;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        AplicarEstado(estadoActual);
    }

    private void Update()
    {
        if (jugadorEnZona && estadoActual == Estado.ELECTRIFICADO && playerController != null)
        {
            timerDanio -= Time.deltaTime;
            if (timerDanio <= 0f)
            {
                playerController.TakeDamage();
                timerDanio = danioCooldown;
            }
        }
    }

    public void SetEstado(Estado nuevoEstado)
    {
        estadoActual = nuevoEstado;
        AplicarEstado(estadoActual);
    }

    public Estado GetEstado()
    {
        return estadoActual;
    }

    private void AplicarEstado(Estado estado)
    {
        switch (estado)
        {
            case Estado.NORMAL:
                if (rend != null && materialNormal != null)
                    rend.sharedMaterial = materialNormal;
                if (particulas != null) particulas.Stop();
                if (audioElectrico != null) audioElectrico.Stop();
                break;

            case Estado.ELECTRIFICADO:
                if (rend != null && materialElectrificado != null)
                {
                    rend.sharedMaterial = materialElectrificado;
                    if (rend.material.HasProperty("_EmissionColor"))
                    {
                        rend.material.EnableKeyword("_EMISSION");
                        rend.material.SetColor("_EmissionColor", Color.cyan * 3f);
                    }
                }
                if (particulas != null) particulas.Play();
                if (audioElectrico != null) audioElectrico.Play();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnZona = true;
        playerController = other.GetComponent<SubLevelPlayerController>();
        timerDanio = 0f;

        if (estadoActual == Estado.ELECTRIFICADO && playerController != null)
        {
            playerController.TakeDamage();
            timerDanio = danioCooldown;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnZona = false;
        playerController = null;
    }
}
