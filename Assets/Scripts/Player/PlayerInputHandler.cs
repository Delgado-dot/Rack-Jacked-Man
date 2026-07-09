using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;


    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }

    public bool ShootPressed { get; private set; }

    public bool Sprinting { get; private set; }


    private void Awake()
    {
        controls = new PlayerControls();
    }


    private void OnEnable()
    {
        controls.Enable();


        controls.Player.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<Vector2>();
        };


        controls.Player.Move.canceled += ctx =>
        {
            MoveInput = Vector2.zero;
        };


        controls.Player.Jump.performed += ctx =>
        {
            JumpPressed = true;
        };


        controls.Player.Sprint.performed += ctx =>
        {
            Sprinting = true;
        };


        controls.Player.Sprint.canceled += ctx =>
        {
            Sprinting = false;
        };


        controls.Player.Shoot.performed += ctx =>
        {
            ShootPressed = true;
        };
    }


    private void OnDisable()
    {
        controls.Disable();
    }


    public void ConsumeJump()
    {
        JumpPressed = false;
    }


    public void ConsumeShoot()
    {
        ShootPressed = false;
    }
}