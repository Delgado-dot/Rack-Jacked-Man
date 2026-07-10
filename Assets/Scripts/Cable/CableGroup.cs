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
    [SerializeField] private float activeDuration = 0.5f;

    [Header("Cooldown Phase")]
    [SerializeField] private float cooldownDuration = 0.1f;

    [Header("Idle Timing")]
    [SerializeField] private float minTimeBetweenCycles = 8f;
    [SerializeField] private float maxTimeBetweenCycles = 20f;

    [Header("Global Limit")]
    [SerializeField] private int maxCablesActivos = 2;

    private GroupState currentState = GroupState.Idle;
    private float stateTimer = 0f;
    private float nextCycleTime;
    private int currentWarningStep = 0;
    private ElectrifiedCable[] allCables;

    private void Awake()
    {
        allCables = new ElectrifiedCable[] { cablePrincipal, extremoIzquierdo, extremoDerecho };

        foreach (var cable in allCables)
        {
            if (cable != null)
                cable.SetManagedByGroup(true);
        }
    }

    private void Start()
    {
        ScheduleNextCycle();
    }

    private void Update()
    {
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

    // --- State machine ---

    private void UpdateIdle()
    {
        if (Time.time >= nextCycleTime)
        {
            if (ElectrifiedCable.GetElectrifiedCount() < maxCablesActivos)
                StartWarning();
            else
                ScheduleNextCycle();
        }
    }

    private void StartWarning()
    {
        currentState = GroupState.Warning;
        stateTimer = 0f;
        currentWarningStep = 0;
        SetAllCables(true, 0f);
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);

        int step = Mathf.FloorToInt(progress * warningSteps);
        if (step != currentWarningStep)
            currentWarningStep = step;

        float t = (float)currentWarningStep / (warningSteps - 1);
        SetAllCables(true, t);

        if (stateTimer >= warningDuration)
            StartActive();
    }

    private void StartActive()
    {
        if (ElectrifiedCable.GetElectrifiedCount() >= maxCablesActivos)
        {
            SkipToCooldown();
            return;
        }

        currentState = GroupState.Active;
        stateTimer = 0f;
        ElectrifiedCable.IncrementElectrifiedCount();
        SetAllCables(true, 1f);
    }

    private void UpdateActive()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= activeDuration)
            StartCooldown();
    }

    private void StartCooldown()
    {
        currentState = GroupState.Cooldown;
        stateTimer = 0f;
        ElectrifiedCable.DecrementElectrifiedCount();
    }

    private void SkipToCooldown()
    {
        currentState = GroupState.Cooldown;
        stateTimer = 0f;
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float t = 1f - Mathf.Clamp01(stateTimer / cooldownDuration);
        SetAllCables(true, t);

        if (stateTimer >= cooldownDuration)
            EndCooldown();
    }

    private void EndCooldown()
    {
        currentState = GroupState.Idle;
        SetAllCables(false, 0f);
        ScheduleNextCycle();
    }

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    // --- Cable synchronization ---

    private void SetAllCables(bool electrified, float intensity)
    {
        foreach (var cable in allCables)
        {
            if (cable != null)
                cable.SetElectrified(electrified, intensity);
        }
    }

    // --- Public API ---

    public GroupState GetCurrentState()
    {
        return currentState;
    }

    public void ForceActivate()
    {
        if (currentState != GroupState.Idle) return;
        StartWarning();
    }

    public void ForceDeactivate()
    {
        if (currentState == GroupState.Active)
            ElectrifiedCable.DecrementElectrifiedCount();

        currentState = GroupState.Idle;
        SetAllCables(false, 0f);
        ScheduleNextCycle();
    }
}
