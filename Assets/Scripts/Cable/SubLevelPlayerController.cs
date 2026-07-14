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

    [Header("Damage Feedback")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private float damageShakeIntensity = 0.15f;
    private float damageFlashTimer = 0f;
    private float shakeTimer = 0f;
    private Renderer playerRenderer;
    private Color originalColor;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Scoring")]
    [SerializeField] private int pointsPerLevel = 500;

    [Header("Teleport Power")]
    [SerializeField] private float teleportMinDistance = 10f;
    [SerializeField] private float teleportMaxDistance = 40f;
    private bool hasPower = false;

    [Header("Teleport Feedback")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.25f;
    private Vector3 cameraBaseLocalPos;

    [Header("Camera Head Bob")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobFrequency = 10f;
    private float bobTimer = 0f;

    [Header("Level Limit")]
    [SerializeField] private float limiteZ = 500f;

    [Header("Camera Lane Tilt")]
    [SerializeField] private float tiltAngle = 1.5f;
    [SerializeField] private float tiltSpeed = 6f;
    private float currentTilt = 0f;
    private float prevTargetX;

    private int currentLane = 1;
    private float targetX;
    private CharacterController cc;
    private bool isSprinting;

    public event System.Action<int> OnScoreChanged;
    public event System.Action OnPlayerDeath;

    private PlayerHealth playerHealth;

    private int score = 0;
    private int combo = 0;
    private float comboTimer = 0f;
    private float comboTimeout = 2f;

    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction sprintAction;
    private InputAction teleportAction;
    private InputAction shootAction;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("[SubLevelPlayerController] No se encontro CharacterController en " + name + "! Agregando uno automaticamente.");
            cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
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
        playerHealth = GetComponent<PlayerHealth>();

        currentLane = 1;
        targetX = lanePositions[currentLane];
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        Camera cam = Camera.main;
        if (cam != null)
            cameraBaseLocalPos = cam.transform.localPosition;

        prevTargetX = targetX;

        OnScoreChanged?.Invoke(score);

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("[SubLevelPlayerController] Start: " +
            "tag=" + tag +
            ", rb=" + (rb != null ? "YES(isKinematic=" + rb.isKinematic + ")" : "NO") +
            ", collider=" + (col != null ? "YES(isTrigger=" + col.isTrigger + ")" : "NO") +
            ", playerHealth=" + (playerHealth != null ? "YES" : "NO"));
    }

    private void Update()
    {
        MoveForward();
        MoveToLane();
        UpdateCameraEffects();
        VerificarLimites();
        UpdateDamageFeedback();
        UpdateCombo();
    }

    private void UpdateCombo()
    {
        if (combo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                combo = 0;
            }
        }
    }

    private void VerificarLimites()
    {
        if (transform.position.z > limiteZ)
        {
            Debug.Log("Limite alcanzado. Subnivel completado.");
            AddScore(pointsPerLevel);
            Time.timeScale = 0f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
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

        float speed = isSprinting ? sprintSpeed : forwardSpeed;
        bobTimer += Time.deltaTime * bobFrequency * (speed / forwardSpeed);
        offset.y += Mathf.Sin(bobTimer) * bobAmplitude;

        float laneDelta = targetX - prevTargetX;
        float targetTilt = laneDelta != 0f ? -Mathf.Sign(laneDelta) * tiltAngle : 0f;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        if (Mathf.Abs(currentTilt) < 0.01f && targetTilt == 0f) currentTilt = 0f;
        cam.transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
        prevTargetX = targetX;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float progress = shakeTimer / shakeDuration;
            float currentIntensity = shakeIntensity * progress;
            offset += Random.insideUnitSphere * currentIntensity;
        }

        cam.transform.localPosition = cameraBaseLocalPos + offset;
    }

    private void UpdateDamageFeedback()
    {
        if (playerHealth == null) return;

        if (playerHealth.IsInvulnerable())
        {
            if (playerRenderer != null)
            {
                float flash = Mathf.Sin(Time.time * 20f) > 0f ? 1f : 0.3f;
                playerRenderer.material.color = Color.Lerp(originalColor, Color.red, flash);
            }
        }
        else if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }

        if (damageFlashTimer > 0f)
        {
            damageFlashTimer -= Time.deltaTime;
        }
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        Vector3 spawnPos = transform.position + Vector3.up * 1f + Vector3.forward * 1.5f;
        Projectile.Spawn(spawnPos, transform.forward);
    }

    public void TakeDamage(int cantidad)
    {
        if (playerHealth == null) return;

        bool wasInvulnerable = playerHealth.IsInvulnerable();
        int livesBefore = playerHealth.GetCurrentLives();

        playerHealth.TakeDamage(cantidad);

        if (playerHealth.GetCurrentLives() < livesBefore && !playerHealth.IsDead())
        {
            Debug.Log("Jugador danado: -" + cantidad + " vida. Restante: " + playerHealth.GetCurrentLives());
            damageFlashTimer = damageFlashDuration;
            shakeTimer = damageShakeIntensity;
        }

        if (playerHealth.IsDead() && !wasInvulnerable)
        {
            Debug.Log("Jugador derrotado.");
            OnPlayerDeath?.Invoke();
        }
    }

    public void AddScore(int points)
    {
        combo++;
        comboTimer = comboTimeout;
        int multiplied = points * Mathf.Max(1, combo);
        score += multiplied;
        OnScoreChanged?.Invoke(score);
        Debug.Log("Score +" + multiplied + " (x" + combo + " combo). Total: " + score);
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
    public int GetCurrentHealth() => playerHealth != null ? playerHealth.GetCurrentLives() : 0;
    public int GetMaxHealth() => playerHealth != null ? playerHealth.GetMaxLives() : 3;
    public int GetScore() => score;
    public int GetCombo() => combo;
    public bool IsInvulnerable() => playerHealth != null && playerHealth.IsInvulnerable();
}
