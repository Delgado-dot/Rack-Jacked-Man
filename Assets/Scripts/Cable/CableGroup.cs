using UnityEngine;
using System.Collections.Generic;

public class CableGroup : MonoBehaviour
{
    public enum GroupState { Idle, Warning, Active, Cooldown }

    [Header("Cables (Component con Renderer)")]
    [SerializeField] private Component cablePrincipal;
    [SerializeField] private Component extremoIzquierdo;
    [SerializeField] private Component extremoDerecho;

    [Header("Warning")]
    [SerializeField] private float warningDuration = 3f;
    [SerializeField] private int warningParpadeos = 5;

    [Header("Active")]
    [SerializeField] private float activeDuration = 2.5f;

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 0.5f;

    [Header("Timing")]
    [SerializeField] private float minTimeBetweenCycles = 8f;
    [SerializeField] private float maxTimeBetweenCycles = 20f;

    [Header("Danio")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Color")]
    [SerializeField] private Color electricColor = new Color(0f, 0.7f, 1f, 1f);

    [Header("Test")]
    [SerializeField] private bool testMode = false;

    private GroupState currentState = GroupState.Idle;
    private float stateTimer = 0f;
    private float nextCycleTime;
    private float nextDamageTime = 0f;
    private Color originalColor;

    private Renderer[] cables;
    private Material[] materials;

    private static int activeGroupCount = 0;
    private static int maxActiveGroups = 2;
    private bool isActiveGroup = false;

    private void Awake()
    {
        CollectRenderers();
        CloneMaterials();
        ClampValues();
    }

    private void Start()
    {
        ScheduleNextCycle();
    }

    private void Update()
    {
        if (testMode && Input.GetKeyDown(KeyCode.T))
        {
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

    // ─── Coleccionar Renderers ────────────────────────────────────

    private void CollectRenderers()
    {
        List<Renderer> list = new List<Renderer>();

        Component[] refs = { cablePrincipal, extremoIzquierdo, extremoDerecho };
        foreach (Component c in refs)
        {
            if (c == null) continue;
            Renderer r = c.GetComponent<Renderer>();
            if (r == null) r = c.GetComponentInChildren<Renderer>();
            if (r != null) list.Add(r);
        }

        if (list.Count > 0)
        {
            cables = list.ToArray();
            return;
        }

        Renderer[] found = GetComponentsInChildren<Renderer>();
        cables = found.Length > 0 ? found : new Renderer[0];
    }

    private void CloneMaterials()
    {
        materials = new Material[cables.Length];
        for (int i = 0; i < cables.Length; i++)
        {
            if (cables[i] == null) continue;
            materials[i] = new Material(cables[i].material);
            cables[i].material = materials[i];
        }

        if (materials.Length > 0 && materials[0] != null)
            originalColor = materials[0].color;
    }

    private void ClampValues()
    {
        warningDuration = Mathf.Max(0.5f, warningDuration);
        warningParpadeos = Mathf.Max(1, warningParpadeos);
        activeDuration = Mathf.Max(0.5f, activeDuration);
        cooldownDuration = Mathf.Max(0.1f, cooldownDuration);
        minTimeBetweenCycles = Mathf.Max(1f, minTimeBetweenCycles);
        maxTimeBetweenCycles = Mathf.Max(minTimeBetweenCycles + 1f, maxTimeBetweenCycles);
    }

    // ─── Idle ─────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        if (Time.time >= nextCycleTime && activeGroupCount < maxActiveGroups)
        {
            EnterWarning();
        }
    }

    // ─── Warning ──────────────────────────────────────────────────

    private void EnterWarning()
    {
        currentState = GroupState.Warning;
        stateTimer = 0f;
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(stateTimer / warningDuration);
        int paso = Mathf.FloorToInt(progress * warningParpadeos);
        bool encendido = paso % 2 == 0;

        SetColor(encendido ? electricColor : originalColor);

        if (stateTimer >= warningDuration)
        {
            EnterActive();
        }
    }

    // ─── Active ───────────────────────────────────────────────────

    private void EnterActive()
    {
        currentState = GroupState.Active;
        stateTimer = 0f;
        nextDamageTime = 0f;
        isActiveGroup = true;
        activeGroupCount++;

        SetColor(electricColor);
    }

    private void UpdateActive()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= activeDuration)
        {
            EnterCooldown();
        }
    }

    // ─── Cooldown ─────────────────────────────────────────────────

    private void EnterCooldown()
    {
        currentState = GroupState.Cooldown;
        stateTimer = 0f;

        if (isActiveGroup)
        {
            activeGroupCount = Mathf.Max(0, activeGroupCount - 1);
            isActiveGroup = false;
        }
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;
        float t = 1f - Mathf.Clamp01(stateTimer / cooldownDuration);
        SetColor(Color.Lerp(originalColor, electricColor, t));

        if (stateTimer >= cooldownDuration)
        {
            EnterIdle();
        }
    }

    private void EnterIdle()
    {
        currentState = GroupState.Idle;
        SetColor(originalColor);
        ScheduleNextCycle();
    }

    // ─── Danio ────────────────────────────────────────────────────

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (currentState != GroupState.Active) return;
        if (Time.time < nextDamageTime) return;

        SubLevelPlayerController slpc = other.GetComponent<SubLevelPlayerController>();
        if (slpc != null)
        {
            slpc.TakeDamage(damageAmount);
        }
        else
        {
            PlayerHealth.TakeDamage(damageAmount);
        }

        nextDamageTime = Time.time + damageCooldown;
    }

    // ─── Utilidades ───────────────────────────────────────────────

    private void ScheduleNextCycle()
    {
        nextCycleTime = Time.time + Random.Range(minTimeBetweenCycles, maxTimeBetweenCycles);
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
                materials[i].color = color;
        }
    }

    // ─── API publica ──────────────────────────────────────────────

    public GroupState GetCurrentState() => currentState;

    public void ForceActivate()
    {
        if (currentState != GroupState.Idle) return;
        EnterWarning();
    }

    public static void SetMaxActiveGroups(int value) => maxActiveGroups = Mathf.Max(1, value);

    // ─── Lifecycle ────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (isActiveGroup)
        {
            activeGroupCount = Mathf.Max(0, activeGroupCount - 1);
            isActiveGroup = false;
        }
    }
}
