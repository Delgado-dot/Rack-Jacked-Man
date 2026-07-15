using UnityEngine;

public class CamaraLibre3D : MonoBehaviour
{
    public float velocidad = 8f;
    public float velocidadRapida = 16f;
    public float sensibilidadMouse = 2f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;

    void Start()
    {
        Vector3 rot = transform.eulerAngles;
        rotacionX = rot.y;
        rotacionY = rot.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        rotacionX += Input.GetAxis("Mouse X") * sensibilidadMouse;
        rotacionY -= Input.GetAxis("Mouse Y") * sensibilidadMouse;
        rotacionY = Mathf.Clamp(rotacionY, -80f, 80f);

        transform.rotation = Quaternion.Euler(rotacionY, rotacionX, 0f);

        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadRapida : velocidad;

        Vector3 movimiento = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movimiento += transform.forward;
        if (Input.GetKey(KeyCode.S)) movimiento -= transform.forward;
        if (Input.GetKey(KeyCode.A)) movimiento -= transform.right;
        if (Input.GetKey(KeyCode.D)) movimiento += transform.right;

        if (Input.GetKey(KeyCode.Space)) movimiento += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) movimiento -= Vector3.up;

        transform.position += Vector3.ClampMagnitude(movimiento, 1f) * velocidadActual * Time.deltaTime;
    }
}
