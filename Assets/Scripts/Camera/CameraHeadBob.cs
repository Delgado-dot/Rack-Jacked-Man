using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    [Header("Bob Parameters")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float sideAmplitude = 0.01f;
    [SerializeField] private float rollAmplitude = 0.5f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Speed Normalization")]
    [SerializeField] private float maxSprintSpeed = 30f;

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float bobTimer;
    private float currentIntensity;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();

        if (characterController == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                characterController = player.GetComponent<CharacterController>();
        }

        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (characterController == null) return;

        float speed = characterController.velocity.magnitude;
        float targetIntensity = Mathf.InverseLerp(0f, maxSprintSpeed, speed);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, smoothSpeed * Time.deltaTime);

        if (currentIntensity < 0.01f)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                baseLocalPosition,
                smoothSpeed * Time.deltaTime
            );

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                baseLocalRotation,
                smoothSpeed * Time.deltaTime
            );
            return;
        }

        bobTimer += Time.deltaTime * bobFrequency * currentIntensity;

        float offsetY = Mathf.Sin(bobTimer) * bobAmplitude * currentIntensity;
        float offsetX = Mathf.Sin(bobTimer * 0.5f) * sideAmplitude * currentIntensity;
        float rollAngle = Mathf.Sin(bobTimer) * rollAmplitude * currentIntensity;

        Vector3 targetPos = baseLocalPosition + new Vector3(offsetX, offsetY, 0f);
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            smoothSpeed * Time.deltaTime
        );

        Quaternion targetRot = baseLocalRotation * Quaternion.Euler(0f, 0f, rollAngle);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            smoothSpeed * Time.deltaTime
        );
    }
}
