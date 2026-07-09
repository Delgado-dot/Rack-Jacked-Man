using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 25f;


    [Header("Salto")]
    public float jumpHeight = 3f;
    public float gravity = -35f;


    [Header("Suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;


    private CharacterController controller;
    private PlayerInputHandler input;
    private PlayerMode playerMode;


    private Vector3 velocity;
    private Vector3 currentMovement;


    private bool isGrounded;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        playerMode = GetComponent<PlayerMode>();
    }


    private void Update()
    {
        DetectGround();

        ApplyGravity();

        HandleJump();

        HandleMovement();
    }


    private void DetectGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundLayer
        );
    }


    private void HandleJump()
    {
        if (!playerMode.IsNormal())
        {
            input.ConsumeJump();
            return;
        }
        if (input.JumpPressed)
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }

            input.ConsumeJump();
        }
    }


    private void HandleMovement()
    {
        if (!playerMode.IsNormal())
        {
            return;
        }

        Vector3 direction = new Vector3(
            input.MoveInput.x,
            0,
            0
        );


        float targetSpeed =
            direction.magnitude * walkSpeed;


        float smooth =
            direction.magnitude > 0
            ? acceleration
            : deceleration;


        currentMovement = Vector3.MoveTowards(
            currentMovement,
            direction * targetSpeed,
            smooth * Time.deltaTime
        );


        controller.Move(
            currentMovement * Time.deltaTime
        );
    }


    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }


        velocity.y += gravity * Time.deltaTime;


        controller.Move(
            velocity * Time.deltaTime
        );
    }
}