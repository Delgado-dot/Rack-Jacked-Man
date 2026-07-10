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

    private GroupState currentState = GroupState.Idle;
    private float stateTimer = 0f;
    private float nextCycleTime;
    private int currentWarningStep = 0;
    private bool groupCountClaimed = false;
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
        if (Time.time >= nextCycleTime && ElectrifiedCable.GetElectrifiedCount() < ElectrifiedCable.GetMaxActiveCables())
            StartWarning();
    }

    private void StartWarning()
    {
        currentState = GroupState.Warning;
        stateTimer = 0f;
        currentWarningStep = 0;
        ElectrifiedCable.IncrementElectrifiedCount();
        groupCountClaimed = true;
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
        {
            StartActive();
        }
    }

    private void StartActive()
    {
        currentState = GroupState.Active;
        stateTimer = 0f;
        SetAllCables(true, 1f);
        SetAllCanDamage(true);
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
        SetAllCanDamage(false);
        if (groupCountClaimed)
        {
            ElectrifiedCable.DecrementElectrifiedCount();
            groupCountClaimed = false;
        }
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

    private void SetAllCanDamage(bool value)
    {
        foreach (var cable in allCables)
        {
            if (cable != null)
                cable.SetCanDamage(value);
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
