using UnityEngine;

/// <summary>
/// CameraFollow - Camara tipo plataforma 3D con SmoothDamp.
/// Sigue al jugador suavemente con un offset configurable.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target;

    [Header("Configuracion de seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -8f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Limites de la camara (opcional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 15f;

    private void Start()
    {
        // Buscar al jugador automaticamente si no se asigno
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }

            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: No se encontro el jugador.");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calcular posicion deseada con offset
        Vector3 desiredPosition = target.position + offset;

        // Aplicar limites si estan habilitados
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Suavizar el movimiento con SmoothDamp
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }
}
