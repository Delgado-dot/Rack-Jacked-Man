using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 25f;

    [Header("Modo de juego")]
    public bool subnivel = false;

    [Header("Salto")]
    public float jumpHeight = 3f;
    public float gravity = -35f;

    [Header("Rotacion")]
    public float rotationSpeed = 360f;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    private CharacterController controller;
    private PlayerControls controls;
    private Animator animator; // <-- AGREGADO

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 currentMovement;
    private bool sprinting;
    private bool shootPressed;
    private bool jumpPressed;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();

        animator = GetComponentInChildren<Animator>(); // <-- AGREGADO
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        controls.Player.Jump.performed += ctx =>
        {
            jumpPressed = true;
        };

        controls.Player.Sprint.performed += ctx =>
        {
            sprinting = true;
        };

        controls.Player.Sprint.canceled += ctx =>
        {
            sprinting = false;
        };

        controls.Player.Shoot.performed += ctx =>
        {
            shootPressed = true;
        };
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // Detectar suelo
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundLayer
        );

        // Mantener pegado al suelo
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }

        // Salto (Espacio)
        if (jumpPressed)
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }
            jumpPressed = false;
        }

        // Movimiento relativo a la dirección del jugador
        float speed = sprinting ? walkSpeed * 1.5f : walkSpeed;

        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);

        Vector3 direction = transform.TransformDirection(inputDirection);
        direction.y = 0;

        // <-- AGREGADO
        if (animator != null)
        {
            animator.SetBool("IsRunning", direction.magnitude > 0.01f);
        }

        float targetSpeed = direction.magnitude * speed;

        float smooth = direction.magnitude > 0 ? acceleration : deceleration;

        currentMovement = Vector3.MoveTowards(
            currentMovement,
            direction * targetSpeed,
            smooth * Time.deltaTime
        );

        controller.Move(currentMovement * Time.deltaTime);

        // Rotacion segun direccion de movimiento
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Disparo (J)
        if (shootPressed)
        {
            Debug.Log("Disparo");
            shootPressed = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}