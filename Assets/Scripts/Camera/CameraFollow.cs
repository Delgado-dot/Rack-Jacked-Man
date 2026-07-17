using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target;

    [Header("Configuracion de seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, 3f);
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Limites de la camara (opcional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 40f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 15f;
    [SerializeField] private float minZ = -10f;
    [SerializeField] private float maxZ = -8f;

    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
                playerObj = GameObject.Find("Player");

            if (playerObj != null)
                target = playerObj.transform;
        }

        if (target != null)
            transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minZ, maxZ);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref smoothVelocity,
            smoothTime
        );

        Vector3 lookTarget = target.position + Vector3.up * 1f;
        transform.LookAt(lookTarget);
    }
}
