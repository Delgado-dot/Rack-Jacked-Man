using UnityEngine;

public class CableGroup : MonoBehaviour
{
    public enum GroupState { Idle, Warning, Active, Cooldown }

    [Header("Cable References")]
    [SerializeField] private ElectrifiedCable cablePrincipal;
    [SerializeField] private ElectrifiedCable extremoIzquierdo;
    [SerializeField] private ElectrifiedCable extremoDerecho;

    [Header("Warning Phase")]
    [SerializeField] private float warningDuration = 5f;
    [SerializeField] private int warningSteps = 5;

    [Header("Active Phase")]
    [SerializeField] private float activeDuration = 2f;

    [Header("Cooldown Phase")]
    [SerializeField] private float cooldownDuration = 0.5f;

    [Header("Idle Timing")]
    [SerializeField] private float minTimeBetweenCycles = 8f;
    [SerializeField] private float maxTimeBetweenCycles = 20f;

    [Header("Test")]
    [SerializeField] private bool testMode = false;

    private GroupState currentState = GroupState.Idle;
    private float stateTimer = 0f;
    private float nextCycleTime;
    private int currentWarningStep = 0;
    private bool groupCountClaimed = false;
    private ElectrifiedCable[] allCables;

    private void Awake()
    {
        AutoDiscoverCables();
        ForceValidValues();

        foreach (var cable in allCables)
        {
            if (cable != null)
                cable.SetManagedByGroup(true);
        }

        Debug.Log("[CableGroup] " + name + " Awake: " + CountNonNullCables() + "/3 cables asignados");
    }

    private void Start()
    {
        ScheduleNextCycle();
        Debug.Log("[CableGroup] " + name + " Start: próximo ciclo en " + (nextCycleTime - Time.time) + "s");
    }

    private void Update()
    {
        if (testMode && Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[CableGroup] " + name + " TEST: Forzar activación");
            ForceActivate();
        }

        switch (currentState)
        {
            case GroupState.Idle:
                UpdateIdle();
                break;
            case GroupState.Warning:
                UpdateWarning();
                break;
            case GroupState.Active:
                UpdateActive();
                break;
            case GroupState.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    private void AutoDiscoverCables()
    {
        if (cablePrincipal != null && extremoIzquierdo != null && extremoDerecho != null)
        {
            allCables = new ElectrifiedCable[] { cablePrincipal, extremoIzquierdo, extremoDerecho };
            return;
        }

        Debug.LogWarning("[CableGroup] " + name + ": Referencias a ElectrifiedCables son null. Buscando hijos...");

        ElectrifiedCable[] found = GetComponentsInChildren<ElectrifiedCable>();

        if (found.Length >= 3)
        {
            cablePrincipal = found[0];
            extremoIzquierdo = found[1];
            extremoDerecho = found[2];
            allCables = new ElectrifiedCable[] { cablePrincipal, extremoIzquierdo, extremoDerecho };
            Debug.Log("[CableGroup] " + name + ": Encontrados " + found.Length + " ElectrifiedCables en hijos");
        }
        else if (found.Length > 0)
        {
            allCables = found;
            Debug.LogWarning("[CableGroup] " + name + ": Solo " + found.Length + " ElectrifiedCables encontrados (se necesitan 3)");
        }
        else
        {
            ElectrifiedCable[] sceneCables = FindObjectsByType<ElectrifiedCable>(FindObjectsInactive.Exclude);
            allCables = sceneCables;
            Debug.LogWarning("[CableGroup] " + name + ": Usando " + sceneCables.Length + " ElectrifiedCables de toda la escena");
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

    private int CountNonNullCables()
    {
        int count = 0;
        if (allCables == null) return 0;
        foreach (var c in allCables)
            if (c != null) count++;
        return count;
    }

    private void UpdateIdle()
    {
        if (Time.time >= nextCycleTime && ElectrifiedCable.GetElectrifiedCount() < ElectrifiedCable.GetMaxActiveCables())
        {
            Debug.Log("[CableGroup] " + name + " IDLE→WARNING (electrifiedCount=" + ElectrifiedCable.GetElectrifiedCount() + ", max=" + ElectrifiedCable.GetMaxActiveCables() + ")");
            StartWarning();
        }
    }

    private void StartWarning()
    {
        currentState = GroupState.Warning;
        stateTimer = 0f;
        currentWarningStep = 0;
        ElectrifiedCable.IncrementElectrifiedCount();
        groupCountClaimed = true;
        SetAllCables(true, 0f);
        Debug.Log("[CableGroup] " + name + " → WARNING (duración=" + warningDuration + "s)");
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);

        int step = Mathf.FloorToInt(progress * warningSteps);
        if (step != currentWarningStep)
            currentWarningStep = step;

        float t = (float)currentWarningStep / Mathf.Max(1, warningSteps - 1);
        SetAllCables(true, t);

        if (stateTimer >= warningDuration)
        {
            Debug.Log("[CableGroup] " + name + " WARNING→ACTIVE");
            StartActive();
        }
    }

    private void StartActive()
    {
        currentState = GroupState.Active;
        stateTimer = 0f;
        SetAllCables(true, 1f);
        SetAllCanDamage(true);
        Debug.Log("[CableGroup] " + name + " → ACTIVE (daño ON, duración=" + activeDuration + "s, cables=" + CountNonNullCables() + ")");
    }

    private void UpdateActive()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= activeDuration)
        {
            Debug.Log("[CableGroup] " + name + " ACTIVE→COOLDOWN");
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        currentState = GroupState.Cooldown;
        stateTimer = 0f;
        SetAllCanDamage(false);
        if (groupCountClaimed)
        {
            ElectrifiedCable.DecrementElectrifiedCount();
            groupCountClaimed = false;
        }
        Debug.Log("[CableGroup] " + name + " → COOLDOWN (duración=" + cooldownDuration + "s)");
    }

    private void SkipActivation()
    {
        SetAllCanDamage(false);
        if (groupCountClaimed)
        {
            ElectrifiedCable.DecrementElectrifiedCount();
            groupCountClaimed = false;
        }
        SetAllCables(false, 0f);
        currentState = GroupState.Idle;
        ScheduleNextCycle();
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float t = 1f - Mathf.Clamp01(stateTimer / cooldownDuration);
        SetAllCables(true, t);

        if (stateTimer >= cooldownDuration)
        {
            Debug.Log("[CableGroup] " + name + " COOLDOWN→IDLE");
            EndCooldown();
        }
    }

    private void EndCooldown()
    {
        currentState = GroupState.Idle;
        SetAllCables(false, 0f);
        ScheduleNextCycle();
        Debug.Log("[CableGroup] " + name + " → IDLE (próximo en " + (nextCycleTime - Time.time) + "s)");
    }

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    private void SetAllCables(bool electrified, float intensity)
    {
        if (allCables == null) return;
        foreach (var cable in allCables)
        {
            if (cable != null)
                cable.SetElectrified(electrified, intensity);
        }
    }

    private void SetAllCanDamage(bool value)
    {
        if (allCables == null) return;
        int setCount = 0;
        foreach (var cable in allCables)
        {
            if (cable != null)
            {
                cable.SetCanDamage(value);
                setCount++;
            }
        }
        Debug.Log("[CableGroup] " + name + " SetCanDamage(" + value + ") → " + setCount + " cables actualizados");
    }

    public GroupState GetCurrentState() => currentState;

    public void ForceActivate()
    {
        if (currentState != GroupState.Idle) return;
        StartWarning();
    }

    public void ForceDeactivate()
    {
        SetAllCanDamage(false);
        if (groupCountClaimed)
        {
            ElectrifiedCable.DecrementElectrifiedCount();
            groupCountClaimed = false;
        }
        SetAllCables(false, 0f);
        currentState = GroupState.Idle;
        ScheduleNextCycle();
    }

    private void OnDestroy()
    {
        if (groupCountClaimed)
        {
            ElectrifiedCable.DecrementElectrifiedCount();
            groupCountClaimed = false;
        }
    }
}
