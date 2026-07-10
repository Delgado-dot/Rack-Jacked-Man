using UnityEngine;

public class ElectrifiedCable : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer cableRenderer;
    [SerializeField] private Color electricColor = new Color(0f, 0.7f, 1f, 1f);
    [SerializeField] private ParticleSystem electricParticles;
    [SerializeField] private Light electricLight;

    [Header("Timing")]
    [SerializeField] private float minTimeBetweenChanges = 5f;
    [SerializeField] private float maxTimeBetweenChanges = 15f;
    [SerializeField] private float electrifyDuration = 3f;

    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    private Material originalMaterial;
    private bool isElectrified = false;
    private float nextChangeTime;
    private float nextDamageTime = 0f;

    private static int electrifiedCount = 0;

    private void Awake()
    {
        if (cableRenderer == null)
            cableRenderer = GetComponent<Renderer>();

        if (cableRenderer != null)
            originalMaterial = cableRenderer.material;

        if (electricParticles != null)
            electricParticles.Stop();

        if (electricLight != null)
            electricLight.enabled = false;
    }

    private void Start()
    {
        ScheduleNextChange();
    }

    private void Update()
    {
        if (!isElectrified && Time.time >= nextChangeTime)
        {
            TryElectrify();
        }
    }

    private void TryElectrify()
    {
        if (electrifiedCount >= 2) return;

        isElectrified = true;
        electrifiedCount++;

        if (cableRenderer != null)
        {
            cableRenderer.material.color = electricColor;
            cableRenderer.material.SetColor("_EmissionColor", electricColor * 3f);
            cableRenderer.material.EnableKeyword("_EMISSION");
        }

        if (electricParticles != null)
            electricParticles.Play();

        if (electricLight != null)
            electricLight.enabled = true;

        Invoke(nameof(Deelectrify), electrifyDuration);
        ScheduleNextChange();
    }

    private void Deelectrify()
    {
        if (!isElectrified) return;

        isElectrified = false;
        electrifiedCount--;

        if (cableRenderer != null && originalMaterial != null)
            cableRenderer.material = originalMaterial;

        if (electricParticles != null)
            electricParticles.Stop();

        if (electricLight != null)
            electricLight.enabled = false;
    }

    private void ScheduleNextChange()
    {
        nextChangeTime = Time.time + Random.Range(minTimeBetweenChanges, maxTimeBetweenChanges);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isElectrified) return;
        if (Time.time < nextDamageTime) return;

        if (other.CompareTag("Player"))
        {
            SubLevelPlayerController player = other.GetComponent<SubLevelPlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
                nextDamageTime = Time.time + damageCooldown;
            }
        }
    }

    public bool IsElectrified()
    {
        return isElectrified;
    }

    private void OnDestroy()
    {
        if (isElectrified)
            electrifiedCount--;
    }
}
