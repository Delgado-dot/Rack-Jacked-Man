using UnityEngine;
using UnityEngine.InputSystem;

public class SubLevelPlayerController : MonoBehaviour
{
    [Header("Lane System")]
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f };
    [SerializeField] private float laneChangeSpeed = 10f;

    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float sprintSpeed = 16f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Teleport Power")]
    [SerializeField] private float teleportDistance = 15f;
    [SerializeField] private float teleportCooldown = 2f;
    private float teleportTimer = 0f;
    private bool hasPower = false;

    private int currentLane = 1;
    private float targetX;
    private CharacterController cc;
    private bool isSprinting;

    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction sprintAction;
    private InputAction teleportAction;

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
        teleportAction.AddBinding("<Keyboard>/space");
        teleportAction.performed += ctx => TryTeleport();
        teleportAction.Enable();
    }

    private void OnDisable()
    {
        moveLeftAction?.Disable();
        moveRightAction?.Disable();
        sprintAction?.Disable();
        teleportAction?.Disable();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentLane = 1;
        targetX = lanePositions[currentLane];
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    private void Update()
    {
        MoveForward();
        MoveToLane();
        UpdateCooldowns();
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

    private void UpdateCooldowns()
    {
        if (teleportTimer > 0f)
            teleportTimer -= Time.deltaTime;
    }

    private void TryTeleport()
    {
        if (hasPower && teleportTimer <= 0f)
        {
            Vector3 pos = transform.position;
            pos.z += teleportDistance;
            transform.position = pos;

            hasPower = false;
            teleportTimer = teleportCooldown;

            Debug.Log("Teletransporte activado. Distancia: " + teleportDistance);
        }
    }

    // === Public API for other scripts ===

    public void TakeDamage()
    {
        currentHealth--;
        Debug.Log("Jugador danado. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Jugador derrotado.");
            Time.timeScale = 0f;
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
