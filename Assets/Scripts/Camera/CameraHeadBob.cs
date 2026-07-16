using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    [Header("Bob Parameters")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float sideAmplitude = 0.01f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Speed Normalization")]
    [SerializeField] private float maxSprintSpeed = 30f;

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
    }

    private void LateUpdate()
    {
        if (characterController == null) return;

        float speed = characterController.velocity.magnitude;
        float targetIntensity = Mathf.InverseLerp(0f, maxSprintSpeed, speed);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, smoothSpeed * Time.deltaTime);

        if (currentIntensity < 0.01f) return;

        bobTimer += Time.deltaTime * bobFrequency * currentIntensity;

        float offsetY = Mathf.Sin(bobTimer) * bobAmplitude * currentIntensity;
        float offsetX = Mathf.Sin(bobTimer * 0.5f) * sideAmplitude * currentIntensity;

        transform.position += new Vector3(offsetX, offsetY, 0f);
    }
}
