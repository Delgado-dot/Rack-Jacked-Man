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
    [SerializeField] private float activeDuration = 0.5f;

    [Header("Cooldown Phase")]
    [SerializeField] private float cooldownDuration = 0.1f;

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
    private int currentWarningStep = 0;
    private float lightBaseIntensity;
    private bool managedByGroup = false;

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
    }

    private void Start()
    {
        if (!managedByGroup)
            ScheduleNextCycle();
    }

    private void Update()
    {
        if (managedByGroup) return;

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

    // --- Autonomous cycle (used when not managed by a group) ---

    private void UpdateInactive()
    {
        if (Time.time >= nextCycleTime)
        {
            if (electrifiedCount < 2)
                StartWarning();
            else
                ScheduleNextCycle();
        }
    }

    private void StartWarning()
    {
        currentState = CableState.Warning;
        stateTimer = 0f;
        currentWarningStep = 0;
        ApplyVisualState(true, 0f);
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);

        int step = Mathf.FloorToInt(progress * warningSteps);
        if (step != currentWarningStep)
            currentWarningStep = step;

        float t = (float)currentWarningStep / (warningSteps - 1);
        ApplyVisualState(true, t);

        if (warningParticles != null && !warningParticles.isPlaying)
            warningParticles.Play();

        if (stateTimer >= warningDuration)
            StartActive();
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
        ApplyVisualState(true, 1f);
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
            StartCooldown();
    }

    private void StartCooldown()
    {
        currentState = CableState.Cooldown;
        stateTimer = 0f;
        electrifiedCount--;
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float t = 1f - Mathf.Clamp01(stateTimer / cooldownDuration);
        ApplyVisualState(true, t);

        if (stateTimer >= cooldownDuration)
            EndCooldown();
    }

    private void EndCooldown()
    {
        currentState = CableState.Inactive;
        ApplyVisualState(false, 0f);
        ScheduleNextCycle();
    }

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    // --- Visual state ---

    private void ApplyVisualState(bool electrified, float intensity)
    {
        if (cableRenderer != null)
        {
            Color targetColor = electrified
                ? Color.Lerp(originalColor, electricColor, intensity)
                : originalColor;

            cableRenderer.material.color = targetColor;

            if (electrified)
            {
                float emissionStrength = intensity * 3f;
                cableRenderer.material.SetColor("_EmissionColor", electricColor * emissionStrength);
                cableRenderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                cableRenderer.material.SetColor("_EmissionColor", Color.black);
                cableRenderer.material.DisableKeyword("_EMISSION");
            }
        }

        if (electricParticles != null)
        {
            if (electrified && intensity >= 1f)
                electricParticles.Play();
            else
                electricParticles.Stop();
        }

        if (warningParticles != null)
        {
            if (electrified && intensity > 0f && intensity < 1f)
            {
                if (!warningParticles.isPlaying)
                    warningParticles.Play();
            }
            else
            {
                warningParticles.Stop();
            }
        }

        if (electricLight != null)
        {
            electricLight.enabled = electrified && intensity > 0f;
            if (electricLight.enabled)
                electricLight.intensity = intensity * lightBaseIntensity;
        }
    }

    // --- Public API for CableGroup and external control ---

    public void SetElectrified(bool value)
    {
        SetElectrified(value, value ? 1f : 0f);
    }

    public void SetElectrified(bool value, float intensity)
    {
        if (value)
        {
            currentState = CableState.Active;
            ApplyVisualState(true, Mathf.Clamp01(intensity));
        }
        else
        {
            currentState = CableState.Inactive;
            ApplyVisualState(false, 0f);
            if (cableRenderer != null && originalMaterial != null)
                cableRenderer.material = originalMaterial;
        }
    }

    public void SetManagedByGroup(bool value)
    {
        managedByGroup = value;
    }

    // --- Damage ---

    private void OnTriggerStay(Collider other)
    {
        if (managedByGroup) return;
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

    // --- Public queries ---

    public CableState GetCurrentState()
    {
        return currentState;
    }

    public bool IsElectrified()
    {
        return currentState == CableState.Active;
    }

    public bool IsManagedByGroup()
    {
        return managedByGroup;
    }

    // --- Static count management for CableGroup coordination ---

    public static int GetElectrifiedCount()
    {
        return electrifiedCount;
    }

    public static void IncrementElectrifiedCount()
    {
        electrifiedCount++;
    }

    public static void DecrementElectrifiedCount()
    {
        electrifiedCount--;
    }

    private void OnDestroy()
    {
        if (currentState == CableState.Active)
            electrifiedCount--;
    }
}
