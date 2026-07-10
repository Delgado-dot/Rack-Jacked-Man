using UnityEngine;

public class ElectrifiedCable : MonoBehaviour
{
    public enum CableState { Inactive, Warning, Active, Cooldown }

    [Header("References")]
    [SerializeField] private Renderer cableRenderer;
    [SerializeField] private Color electricColor = new Color(0f, 0.7f, 1f, 1f);
    [SerializeField] private ParticleSystem electricParticles;
    [SerializeField] private ParticleSystem warningParticles;
    [SerializeField] private Light electricLight;

    [Header("Warning Phase")]
    [SerializeField] private float warningDuration = 5f;
    [SerializeField] private int warningSteps = 5;

    [Header("Active Phase")]
    [SerializeField] private float activeDuration = 2f;

    [Header("Cooldown Phase")]
    [SerializeField] private float cooldownDuration = 2f;

    [Header("Idle Timing")]
    [SerializeField] private float minTimeBetweenCycles = 8f;
    [SerializeField] private float maxTimeBetweenCycles = 20f;

    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    private Material originalMaterial;
    private Color originalColor;
    private CableState currentState = CableState.Inactive;
    private float stateTimer = 0f;
    private float nextDamageTime = 0f;
    private float nextCycleTime;
    private float warningStepDuration;
    private int currentWarningStep = 0;
    private float lightBaseIntensity;

    private static int electrifiedCount = 0;

    private void Awake()
    {
        if (cableRenderer == null)
            cableRenderer = GetComponent<Renderer>();

        if (cableRenderer != null)
        {
            originalMaterial = cableRenderer.material;
            originalColor = originalMaterial.color;
        }

        if (electricParticles != null)
            electricParticles.Stop();

        if (warningParticles != null)
            warningParticles.Stop();

        if (electricLight != null)
        {
            electricLight.enabled = false;
            lightBaseIntensity = electricLight.intensity;
        }

        warningStepDuration = warningDuration / warningSteps;
    }

    private void Start()
    {
        ScheduleNextCycle();
    }

    private void Update()
    {
        switch (currentState)
        {
            case CableState.Inactive:
                UpdateInactive();
                break;
            case CableState.Warning:
                UpdateWarning();
                break;
            case CableState.Active:
                UpdateActive();
                break;
            case CableState.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    private void UpdateInactive()
    {
        if (Time.time >= nextCycleTime)
        {
            if (electrifiedCount < 2)
            {
                StartWarning();
            }
            else
            {
                ScheduleNextCycle();
            }
        }
    }

    private void StartWarning()
    {
        currentState = CableState.Warning;
        stateTimer = 0f;
        currentWarningStep = 0;

        if (warningParticles != null)
            warningParticles.Play();

        if (electricLight != null)
        {
            electricLight.enabled = true;
            electricLight.intensity = 0f;
        }
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);

        int step = Mathf.FloorToInt(progress * warningSteps);
        if (step != currentWarningStep)
        {
            currentWarningStep = step;
        }

        float t = (float)currentWarningStep / (warningSteps - 1);

        if (cableRenderer != null)
        {
            Color currentColor = Color.Lerp(originalColor, electricColor, t);
            cableRenderer.material.color = currentColor;

            float emissionStrength = t * 2f;
            cableRenderer.material.SetColor("_EmissionColor", electricColor * emissionStrength);
            cableRenderer.material.EnableKeyword("_EMISSION");
        }

        if (electricLight != null)
        {
            electricLight.intensity = t * lightBaseIntensity;
        }

        if (stateTimer >= warningDuration)
        {
            StartActive();
        }
    }

    private void StartActive()
    {
        if (electrifiedCount >= 2)
        {
            EndCooldown();
            return;
        }

        currentState = CableState.Active;
        stateTimer = 0f;
        electrifiedCount++;

        if (cableRenderer != null)
        {
            cableRenderer.material.color = electricColor;
            cableRenderer.material.SetColor("_EmissionColor", electricColor * 3f);
            cableRenderer.material.EnableKeyword("_EMISSION");
        }

        if (electricParticles != null)
            electricParticles.Play();

        if (warningParticles != null)
            warningParticles.Stop();

        if (electricLight != null)
            electricLight.intensity = lightBaseIntensity * 2f;
    }

    private void UpdateActive()
    {
        stateTimer += Time.deltaTime;

        if (electricLight != null)
        {
            float flicker = Mathf.Sin(Time.time * 20f) * 0.3f + 0.7f;
            electricLight.intensity = lightBaseIntensity * 2f * flicker;
        }

        if (stateTimer >= activeDuration)
        {
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        currentState = CableState.Cooldown;
        stateTimer = 0f;
        electrifiedCount--;

        if (electricParticles != null)
            electricParticles.Stop();
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / cooldownDuration);

        float t = 1f - progress;

        if (cableRenderer != null)
        {
            Color currentColor = Color.Lerp(originalColor, electricColor, t);
            cableRenderer.material.color = currentColor;

            float emissionStrength = t * 3f;
            cableRenderer.material.SetColor("_EmissionColor", electricColor * emissionStrength);
            cableRenderer.material.EnableKeyword("_EMISSION");
        }

        if (electricLight != null)
        {
            electricLight.intensity = t * lightBaseIntensity;
        }

        if (stateTimer >= cooldownDuration)
        {
            EndCooldown();
        }
    }

    private void EndCooldown()
    {
        currentState = CableState.Inactive;

        if (cableRenderer != null && originalMaterial != null)
            cableRenderer.material = originalMaterial;

        if (electricLight != null)
            electricLight.enabled = false;

        ScheduleNextCycle();
    }

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentState != CableState.Active) return;
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

    public CableState GetCurrentState()
    {
        return currentState;
    }

    public bool IsElectrified()
    {
        return currentState == CableState.Active;
    }

    private void OnDestroy()
    {
        if (currentState == CableState.Active)
            electrifiedCount--;
    }
}
