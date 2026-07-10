using UnityEngine;
using UnityEngine.InputSystem;

public class SubLevelPlayerController : MonoBehaviour
{
    [Header("Lane System")]
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f };
    [SerializeField] private float laneChangeSpeed = 55f;

    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float sprintSpeed = 30f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    // Integration: Connect to Kevin's health system
    // public event System.Action<int> OnHealthChanged;
    // public event System.Action OnPlayerDeath;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Teleport Power")]
    [SerializeField] private float teleportMinDistance = 10f;
    [SerializeField] private float teleportMaxDistance = 40f;
    private bool hasPower = false;

    [Header("Teleport Feedback")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.25f;
    private float shakeTimer = 0f;
    private Vector3 cameraBaseLocalPos;

    [Header("Camera Head Bob")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobFrequency = 10f;
    private float bobTimer = 0f;

    [Header("Camera Lane Tilt")]
    [SerializeField] private float tiltAngle = 1.5f;
    [SerializeField] private float tiltSpeed = 6f;
    private float currentTilt = 0f;
    private float prevTargetX;

    private int currentLane = 1;
    private float targetX;
    private CharacterController cc;
    private bool isSprinting;

    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction sprintAction;
    private InputAction teleportAction;
    private InputAction shootAction;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveLeftAction = new InputAction("MoveLeft", InputActionType.Button);
        moveLeftAction.AddBinding("<Keyboard>/a");
        moveLeftAction.AddBinding("<Keyboard>/leftArrow");
        moveLeftAction.performed += ctx => ChangeLane(-1);
        moveLeftAction.Enable();

        moveRightAction = new InputAction("MoveRight", InputActionType.Button);
        moveRightAction.AddBinding("<Keyboard>/d");
        moveRightAction.AddBinding("<Keyboard>/rightArrow");
        moveRightAction.performed += ctx => ChangeLane(1);
        moveRightAction.Enable();

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.performed += ctx => isSprinting = true;
        sprintAction.canceled += ctx => isSprinting = false;
        sprintAction.Enable();

        teleportAction = new InputAction("Teleport", InputActionType.Button);
        teleportAction.AddBinding("<Mouse>/leftButton");
        teleportAction.performed += ctx => TryTeleport();
        teleportAction.Enable();

        shootAction = new InputAction("Shoot", InputActionType.Button);
        shootAction.AddBinding("<Keyboard>/space");
        shootAction.performed += ctx => Shoot();
        shootAction.Enable();
    }

    private void OnDisable()
    {
        moveLeftAction?.Disable();
        moveRightAction?.Disable();
        sprintAction?.Disable();
        teleportAction?.Disable();
        shootAction?.Disable();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentLane = 1;
        targetX = lanePositions[currentLane];
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        Camera cam = Camera.main;
        if (cam != null)
            cameraBaseLocalPos = cam.transform.localPosition;

        prevTargetX = targetX;
    }

    private void Update()
    {
        MoveForward();
        MoveToLane();
        UpdateCameraEffects();
    }

    private void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        if (newLane >= 0 && newLane < lanePositions.Length)
        {
            currentLane = newLane;
            targetX = lanePositions[currentLane];
        }
    }

    private void MoveForward()
    {
        float speed = isSprinting ? sprintSpeed : forwardSpeed;
        Vector3 move = Vector3.forward * speed * Time.deltaTime;
        cc.Move(move);
    }

    private void MoveToLane()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void TryTeleport()
    {
        if (!hasPower) return;

        Vector3 startPos = transform.position;
        SpawnTeleportEffect(startPos, true);

        float distance = Random.Range(teleportMinDistance, teleportMaxDistance);
        Vector3 pos = transform.position;
        pos.z += distance;

        int newLane = Random.Range(0, lanePositions.Length);
        currentLane = newLane;
        targetX = lanePositions[currentLane];
        pos.x = targetX;

        transform.position = pos;

        SpawnTeleportEffect(transform.position, false);

        hasPower = false;
        shakeTimer = shakeDuration;
        Debug.Log("Teletransporte activado. Distancia: " + distance + " Carril: " + currentLane);
    }

    private void SpawnTeleportEffect(Vector3 position, bool isExit)
    {
        GameObject effect = new GameObject("TeleportEffect");
        effect.transform.position = position;

        ParticleSystem ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = isExit ? 0.4f : 0.3f;
        main.startSpeed = isExit ? 2f : 4f;
        main.startSize = isExit ? 0.15f : 0.2f;
        main.startColor = new Color(0.3f, 0.6f, 1f, 0.8f);
        main.maxParticles = isExit ? 20 : 30;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)(isExit ? 20 : 30))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = isExit ? 0.3f : 0.5f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        Color teleportColor = new Color(0.3f, 0.6f, 1f, 1f);
        renderer.material.color = teleportColor;
        renderer.material.SetColor("_EmissionColor", teleportColor * 4f);
        renderer.material.EnableKeyword("_EMISSION");
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        float lifetime = isExit ? 0.5f : 0.4f;
        Destroy(effect, lifetime);
    }

    private void UpdateCameraEffects()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 offset = Vector3.zero;

        // Head bob
        float speed = isSprinting ? sprintSpeed : forwardSpeed;
        bobTimer += Time.deltaTime * bobFrequency * (speed / forwardSpeed);
        offset.y += Mathf.Sin(bobTimer) * bobAmplitude;

        // Lane tilt
        float laneDelta = targetX - prevTargetX;
        float targetTilt = laneDelta != 0f ? -Mathf.Sign(laneDelta) * tiltAngle : 0f;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        if (Mathf.Abs(currentTilt) < 0.01f && targetTilt == 0f) currentTilt = 0f;
        cam.transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
        prevTargetX = targetX;

        // Shake
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float progress = shakeTimer / shakeDuration;
            float currentIntensity = shakeIntensity * progress;
            offset += Random.insideUnitSphere * currentIntensity;
        }

        cam.transform.localPosition = cameraBaseLocalPos + offset;
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        Vector3 spawnPos = transform.position + Vector3.up * 1f + Vector3.forward * 1.5f;
        Projectile.Spawn(spawnPos, transform.forward);
    }

    // === Public API ===

    public void TakeDamage(int cantidad)
    {
        currentHealth -= cantidad;
        Debug.Log("Jugador danado: -" + cantidad + " vida. Restante: " + currentHealth);

        // Integration: Notify Kevin's health system
        // OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Jugador derrotado.");
            currentHealth = 0;
            Time.timeScale = 0f;
            // Integration: Trigger death event
            // OnPlayerDeath?.Invoke();
        }
    }

    public void ActivarPoder()
    {
        hasPower = true;
        Debug.Log("Poder de teletransporte activado.");
    }

    public bool TienePoder()
    {
        return hasPower;
    }

    public int GetCurrentLane() => currentLane;
    public float GetTargetX() => targetX;
    public float GetForwardSpeed() => isSprinting ? sprintSpeed : forwardSpeed;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
