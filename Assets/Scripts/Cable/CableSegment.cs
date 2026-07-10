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
    [SerializeField] private int danoPorSegundo = 1;
    [SerializeField] private float danioCooldown = 0.5f;
    private float timerDanio = 0f;

    private Renderer rend;
    private Material runtimeMat;
    private bool jugadorEnZona = false;
    private SubLevelPlayerController playerController;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
            runtimeMat = new Material(rend.sharedMaterial);
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
                playerController.TakeDamage(danoPorSegundo);
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
        if (rend == null) return;

        switch (estado)
        {
            case Estado.NORMAL:
                if (materialNormal != null)
                    runtimeMat = new Material(materialNormal);
                rend.material = runtimeMat;
                if (particulas != null) particulas.Stop();
                if (audioElectrico != null) audioElectrico.Stop();
                break;

            case Estado.ELECTRIFICADO:
                if (materialElectrificado != null)
                    runtimeMat = new Material(materialElectrificado);
                runtimeMat.EnableKeyword("_EMISSION");
                runtimeMat.SetColor("_EmissionColor", Color.cyan * 3f);
                rend.material = runtimeMat;
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
            playerController.TakeDamage(danoPorSegundo);
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
