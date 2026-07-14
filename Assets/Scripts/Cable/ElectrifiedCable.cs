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
    [SerializeField] private float cooldownDuration = 0.1f;

    [Header("Idle Timing")]
    [SerializeField] private float minTimeBetweenCycles = 8f;
    [SerializeField] private float maxTimeBetweenCycles = 20f;

    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    private Material originalMaterial;
    private Material runtimeMaterial;
    private Color originalColor;
    private CableState currentState = CableState.Inactive;
    private float stateTimer = 0f;
    private float nextDamageTime = 0f;
    private float nextCycleTime;
    private float lightBaseIntensity;
    private bool managedByGroup = false;
    private bool countClaimed = false;
    private bool canDamage = false;

    private static int electrifiedCount = 0;
    private static int maxActiveCables = 2;

    private void Awake()
    {
        EnsureCollider();
        ForceValidValues();

        if (cableRenderer == null)
            cableRenderer = GetComponent<Renderer>();

        if (cableRenderer != null)
        {
            if (cableRenderer.material == null)
            {
                cableRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
            originalMaterial = cableRenderer.material;
            originalColor = originalMaterial.color;
            runtimeMaterial = new Material(originalMaterial);
            cableRenderer.material = runtimeMaterial;
        }

        CreateFallbackEffects();

        Debug.Log("[ElectrifiedCable] " + name + " Awake: collider=" + (GetComponent<Collider>() != null) +
            ", managedByGroup=" + managedByGroup + ", canDamage=" + canDamage);
    }

    private void Start()
    {
        if (!managedByGroup)
            ScheduleNextCycle();
    }

    private void EnsureCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1f, 2f, 1f);
            box.center = Vector3.zero;
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void ForceValidValues()
    {
        warningDuration = Mathf.Max(1f, warningDuration);
        warningSteps = Mathf.Max(2, warningSteps);
        activeDuration = Mathf.Max(0.5f, activeDuration);
        cooldownDuration = Mathf.Max(0.1f, cooldownDuration);
        minTimeBetweenCycles = Mathf.Max(1f, minTimeBetweenCycles);
        maxTimeBetweenCycles = Mathf.Max(minTimeBetweenCycles + 1f, maxTimeBetweenCycles);
    }

    private void CreateFallbackEffects()
    {
        if (electricLight == null)
        {
            GameObject lightObj = new GameObject("ElectricLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = Vector3.up * 0.5f;
            electricLight = lightObj.AddComponent<Light>();
            electricLight.type = LightType.Point;
            electricLight.color = electricColor;
            electricLight.intensity = 2f;
            electricLight.range = 3f;
            electricLight.enabled = false;
        }
        lightBaseIntensity = electricLight.intensity;

        if (electricParticles == null)
        {
            GameObject particlesObj = new GameObject("ElectricParticles");
            particlesObj.transform.SetParent(transform, false);
            particlesObj.transform.localPosition = Vector3.up * 0.3f;
            electricParticles = particlesObj.AddComponent<ParticleSystem>();
            var main = electricParticles.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 1f;
            main.startSize = 0.05f;
            main.startColor = electricColor;
            main.maxParticles = 15;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = electricParticles.emission;
            emission.rateOverTime = 10;
            var shape = electricParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            var renderer = particlesObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = electricColor;
            renderer.material.SetColor("_EmissionColor", electricColor * 3f);
            renderer.material.EnableKeyword("_EMISSION");
            electricParticles.Stop();
        }

        if (warningParticles == null)
        {
            GameObject warnObj = new GameObject("WarningParticles");
            warnObj.transform.SetParent(transform, false);
            warnObj.transform.localPosition = Vector3.up * 0.3f;
            warningParticles = warnObj.AddComponent<ParticleSystem>();
            var main = warningParticles.main;
            main.startLifetime = 0.2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.03f;
            main.startColor = Color.yellow;
            main.maxParticles = 8;
            main.loop = true;
            var emission = warningParticles.emission;
            emission.rateOverTime = 5;
            var shape = warningParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            var renderer = warnObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.yellow;
            warningParticles.Stop();
        }
    }

    private void Update()
    {
        if (managedByGroup) return;

        switch (currentState)
        {
            case CableState.Inactive:
                if (Time.time >= nextCycleTime && electrifiedCount < maxActiveCables)
                    StartWarning();
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

    private void StartWarning()
    {
        currentState = CableState.Warning;
        stateTimer = 0f;
        electrifiedCount++;
        countClaimed = true;
        ApplyVisualState(0f);
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);
        int step = Mathf.FloorToInt(progress * warningSteps);
        float t = (float)step / Mathf.Max(1, warningSteps - 1);
        ApplyVisualState(t);

        if (warningParticles != null && t > 0f && !warningParticles.isPlaying)
            warningParticles.Play();

        if (stateTimer >= warningDuration)
            StartActive();
    }

    private void StartActive()
    {
        currentState = CableState.Active;
        stateTimer = 0f;
        ApplyVisualState(1f);
        canDamage = true;
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
        canDamage = false;
        electrifiedCount--;
        countClaimed = false;
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float t = 1f - Mathf.Clamp01(stateTimer / cooldownDuration);
        ApplyVisualState(t);

        if (stateTimer >= cooldownDuration)
        {
            currentState = CableState.Inactive;
            ResetVisual();
            ScheduleNextCycle();
        }
    }

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    private void ApplyVisualState(float intensity)
    {
        if (cableRenderer != null && runtimeMaterial != null)
        {
            runtimeMaterial.color = Color.Lerp(originalColor, electricColor, intensity);
            float emissionStrength = intensity * 3f;
            runtimeMaterial.SetColor("_EmissionColor", electricColor * emissionStrength);
            runtimeMaterial.EnableKeyword("_EMISSION");
        }
        if (electricLight != null)
        {
            electricLight.enabled = intensity > 0f;
            if (electricLight.enabled)
                electricLight.intensity = intensity * lightBaseIntensity;
        }
    }

    private void ResetVisual()
    {
        if (cableRenderer != null && runtimeMaterial != null)
        {
            runtimeMaterial.color = originalColor;
            runtimeMaterial.SetColor("_EmissionColor", Color.black);
        }
        if (electricParticles != null) electricParticles.Stop();
        if (warningParticles != null) warningParticles.Stop();
        if (electricLight != null) electricLight.enabled = false;
    }

    public void SetElectrified(bool value)
    {
        ApplyVisualState(value ? 1f : 0f);
        if (!value) ResetVisual();
    }

    public void SetElectrified(bool value, float intensity)
    {
        if (value)
            ApplyVisualState(Mathf.Clamp01(intensity));
        else
            ResetVisual();
    }

    public void SetManagedByGroup(bool value)
    {
        managedByGroup = value;
    }

    public void SetCanDamage(bool value)
    {
        canDamage = value;
        Debug.Log("[ElectrifiedCable] " + name + " SetCanDamage(" + value + ") currentState=" + currentState);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!canDamage) return;
        if (Time.time < nextDamageTime) return;

        Debug.Log("[ElectrifiedCable] " + name + " → DAÑANDO a " + other.name + " (canDamage=" + canDamage + ")");
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
        nextDamageTime = Time.time + damageCooldown;
    }

    public CableState GetCurrentState() => currentState;
    public bool IsElectrified() => currentState == CableState.Active;
    public bool IsManagedByGroup() => managedByGroup;
    public bool CanDamage() => canDamage;
    public static int GetElectrifiedCount() => electrifiedCount;
    public static int GetMaxActiveCables() => maxActiveCables;
    public static void SetMaxActiveCables(int value) => maxActiveCables = Mathf.Max(1, value);
    public static void IncrementElectrifiedCount() => electrifiedCount++;
    public static void DecrementElectrifiedCount() => electrifiedCount = Mathf.Max(0, electrifiedCount - 1);

    private void OnDestroy()
    {
        if (countClaimed)
        {
            electrifiedCount--;
            countClaimed = false;
        }
    }
}
