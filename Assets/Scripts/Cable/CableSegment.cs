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
    private PlayerHealth playerHealth;

    private void Awake()
    {
        ElectrifiedCable electrified = GetComponent<ElectrifiedCable>();
        if (electrified != null)
        {
            Debug.Log("[CableSegment] ElectrifiedCable detectado en " + name + ". Deshabilitando CableSegment para evitar doble daño.");
            enabled = false;
            return;
        }

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
        if (jugadorEnZona && estadoActual == Estado.ELECTRIFICADO)
        {
            timerDanio -= Time.deltaTime;
            if (timerDanio <= 0f)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(danoPorSegundo);
                }
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
                if (runtimeMat != null) Destroy(runtimeMat);
                if (materialNormal != null)
                    runtimeMat = new Material(materialNormal);
                else
                    runtimeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                rend.material = runtimeMat;
                if (particulas != null) particulas.Stop();
                if (audioElectrico != null) audioElectrico.Stop();
                break;

            case Estado.ELECTRIFICADO:
                if (runtimeMat != null) Destroy(runtimeMat);
                if (materialElectrificado != null)
                    runtimeMat = new Material(materialElectrificado);
                else
                {
                    runtimeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    runtimeMat.color = Color.cyan;
                }
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
        playerHealth = other.GetComponent<PlayerHealth>();
        timerDanio = 0f;

        if (estadoActual == Estado.ELECTRIFICADO)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(danoPorSegundo);
            }
            timerDanio = danioCooldown;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnZona = false;
        playerHealth = null;
    }
}
