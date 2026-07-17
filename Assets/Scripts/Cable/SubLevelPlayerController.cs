using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SubLevelPlayerController : MonoBehaviour
{
    [Header("Carriles")]
    [SerializeField] private float[] lanePositions = { -2.3f, 0f, 2.3f };
    [SerializeField] private float laneChangeSpeed = 55f;

    [Header("Movimiento")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float sprintSpeed = 30f;

    [Header("Camara")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float tiltAngle = 1.5f;
    [SerializeField] private float tiltSpeed = 6f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.25f;

    [Header("Feedback de danio")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private float damageShakeIntensity = 0.15f;

    [Header("Disparo")]
    [SerializeField] private float fireRate = 0.2f;

    [Header("Scoring")]
    [SerializeField] private int pointsPerLevel = 500;

    [Header("Teleport")]
    [SerializeField] private float teleportMinDistance = 10f;
    [SerializeField] private float teleportMaxDistance = 40f;

    [Header("Limite")]
    [SerializeField] private float limiteZ = 500f;

    private int currentLane = 1;
    private float targetX;
    private CharacterController cc;
    private bool isSprinting;
    private bool hasPower = false;

    private float nextFireTime = 0f;
    private int score = 0;
    private int combo = 0;
    private float comboTimer = 0f;
    private float comboTimeout = 2f;

    private float bobTimer = 0f;
    private float currentTilt = 0f;
    private float prevTargetX;
    private float shakeTimer = 0f;
    private float damageFlashTimer = 0f;
    private Vector3 cameraBaseLocalPos;

    private Renderer playerRenderer;
    private Color originalColor;

    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction sprintAction;
    private InputAction teleportAction;
    private InputAction shootAction;

    public event System.Action<int> OnScoreChanged;
    public event System.Action OnPlayerDeath;

    // ─── Lifecycle ────────────────────────────────────────────────

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
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
        currentLane = 1;
        targetX = lanePositions[currentLane];
        cc.enabled = false;
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        cc.enabled = true;

        Camera cam = Camera.main;
        if (cam != null)
            cameraBaseLocalPos = cam.transform.localPosition;

        prevTargetX = targetX;
        OnScoreChanged?.Invoke(score);
    }

    private void Update()
    {
        if (teleportAction.triggered)
            TryTeleport();

        ApplyMovement();
        UpdateCamera();
        UpdateDamageFeedback();
        UpdateCombo();
        CheckLimits();
    }

    // ─── Movimiento ───────────────────────────────────────────────

    private void ApplyMovement()
    {
        float speed = isSprinting ? sprintSpeed : forwardSpeed;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float deltaX = newX - transform.position.x;

        Vector3 move = Vector3.forward * speed * Time.deltaTime + Vector3.right * deltaX;
        cc.Move(move);
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

    // ─── Camara ───────────────────────────────────────────────────

    private void UpdateCamera()
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
            offset += Random.insideUnitSphere * shakeIntensity * progress;
        }

        cam.transform.localPosition = cameraBaseLocalPos + offset;
    }

    // ─── Disparo ──────────────────────────────────────────────────

    private void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        Vector3 spawnPos = transform.position + Vector3.up * 0.1f + Vector3.forward * 1.5f;
        Projectile.Spawn(spawnPos, transform.forward);
    }

    // ─── Teleport ─────────────────────────────────────────────────

    private void TryTeleport()
    {
        if (!hasPower) return;

        float distance = Random.Range(teleportMinDistance, teleportMaxDistance);

        int newLane = Random.Range(0, lanePositions.Length);
        currentLane = newLane;
        targetX = lanePositions[currentLane];

        Vector3 delta = Vector3.zero;
        delta.z = distance;
        delta.x = targetX - transform.position.x;
        cc.Move(delta);

        hasPower = false;
        shakeTimer = shakeDuration;
    }

    // ─── Danio ────────────────────────────────────────────────────

    public void TakeDamage(int cantidad)
    {
        if (PlayerHealth.IsDead()) return;

        bool wasInvulnerable = PlayerHealth.IsInvulnerable();
        int livesBefore = PlayerHealth.GetCurrentLives();

        PlayerHealth.TakeDamage(cantidad);

        if (PlayerHealth.GetCurrentLives() < livesBefore && !PlayerHealth.IsDead())
        {
            damageFlashTimer = damageFlashDuration;
            shakeTimer = damageShakeIntensity;
        }

        if (PlayerHealth.IsDead() && !wasInvulnerable)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    private void UpdateDamageFeedback()
    {
        if (PlayerHealth.IsInvulnerable())
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
            damageFlashTimer -= Time.deltaTime;
    }

    // ─── Scoring ──────────────────────────────────────────────────

    public void AddScore(int points)
    {
        combo++;
        comboTimer = comboTimeout;
        int multiplied = points * Mathf.Max(1, combo);
        score += multiplied;
        OnScoreChanged?.Invoke(score);
    }

    private void UpdateCombo()
    {
        if (combo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                combo = 0;
        }
    }

    // ─── Limites ──────────────────────────────────────────────────

    private void CheckLimits()
    {
        if (transform.position.z > limiteZ)
        {
            AddScore(pointsPerLevel);

            if (GameManager.Instance != null)
            {
                string destino = GameManager.Instance.GetSubLevelDestination();
                Debug.Log("[Runner] Limite alcanzado. Destino: " + destino);
                GameManager.Instance.AvanzarNivel();
                UnityEngine.SceneManagement.SceneManager.LoadScene(destino);
            }
        }
    }

    // ─── Poder ────────────────────────────────────────────────────

    public void ActivarPoder() { hasPower = true; }
    public bool TienePoder() => hasPower;

    // ─── Getters ──────────────────────────────────────────────────

    public int GetScore() => score;
    public int GetCombo() => combo;
    public int GetCurrentLane() => currentLane;
    public float GetTargetX() => targetX;
}
