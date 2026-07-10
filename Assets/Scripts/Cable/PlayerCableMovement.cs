using UnityEngine;

/// <summary>
/// PlayerCableMovement - Movimiento del jugador en el subnivel de cables.
/// Avanza continuamente hacia adelante.
/// Cambia entre tres carriles (izquierdo, centro, derecho).
/// Puede teletransportarse hacia adelante con la chaqueta.
/// NO usa el PlayerMovement normal.
/// </summary>
public class PlayerCableMovement : MonoBehaviour
{
    [Header("Carriles")]
    [SerializeField] private float distanciaCarriles = 3f;
    [SerializeField] private float suavidadCambio = 8f;
    private int carrilActual = 1; // 0=izq, 1=center, 2=der
    private float[] posicionesX;
    private float objetivoX;

    [Header("Movimiento")]
    [SerializeField] private float velocidadBase = 8f;
    [SerializeField] private float velocidadMinima = 5f;
    [SerializeField] private float velocidadMaxima = 15f;
    private float velocidadActual;

    [Header("Teletransporte (Chaqueta)")]
    [SerializeField] private float distanciaTeletransporte = 15f;
    [SerializeField] private float cooldownTeletransporte = 2f;
    private float timerTeletransporte = 0f;
    private bool tienePoder = false;

    [Header("Limites")]
    [SerializeField] private float limiteZ = 500f;

    private CharacterController cc;
    private Vector3 movimientoVertical;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        InicializarCarriles();
        velocidadActual = velocidadBase;
    }

    private void InicializarCarriles()
    {
        posicionesX = new float[3];
        posicionesX[0] = -distanciaCarriles;
        posicionesX[1] = 0f;
        posicionesX[2] = distanciaCarriles;
        objetivoX = posicionesX[carrilActual];
    }

    private void Update()
    {
        ProcesarInput();
        MoverAdelante();
        CambiarCarril();
        ActualizarCooldowns();
        VerificarLimites();
    }

    private void ProcesarInput()
    {
        // Cambio de carril con flechas o A/D
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (carrilActual > 0) carrilActual--;
            objetivoX = posicionesX[carrilActual];
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (carrilActual < 2) carrilActual++;
            objetivoX = posicionesX[carrilActual];
        }

        // Teletransporte con espacio (si tiene poder)
        if (Input.GetKeyDown(KeyCode.Space) && tienePoder && timerTeletransporte <= 0f)
        {
            Teletransportar();
        }
    }

    private void MoverAdelante()
    {
        Vector3 movimiento = transform.forward * velocidadActual * Time.deltaTime;

        // Gravedad basica
        if (!cc.isGrounded)
        {
            movimientoVertical.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            movimientoVertical.y = 0f;
        }

        cc.Move(movimiento + movimientoVertical * Time.deltaTime);
    }

    private void CambiarCarril()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, objetivoX, suavidadCambio * Time.deltaTime);
        transform.position = pos;
    }

    private void Teletransportar()
    {
        Vector3 pos = transform.position;
        pos.z += distanciaTeletransporte;
        transform.position = pos;

        tienePoder = false;
        timerTeletransporte = cooldownTeletransporte;

        Debug.Log("Teletransporte activado. Distancia: " + distanciaTeletransporte);
    }

    private void ActualizarCooldowns()
    {
        if (timerTeletransporte > 0f)
            timerTeletransporte -= Time.deltaTime;
    }

    private void VerificarLimites()
    {
        if (transform.position.z > limiteZ)
        {
            Debug.Log("Subnivel completado.");
            Time.timeScale = 0f;
        }
    }

    public void ActivarPoder()
    {
        tienePoder = true;
        Debug.Log("Poder de teletransporte activado.");
    }

    public bool TienePoder()
    {
        return tienePoder;
    }

    public int GetCarrilActual()
    {
        return carrilActual;
    }

    public float GetVelocidad()
    {
        return velocidadActual;
    }
}
